"""
ICICI EazyPGVerify recon job for affiliated exam-fee payments.

PDF (bank/eazypay.pdf section 8): status by merchantid + pgreferenceno (refno)
  https://eazypay.icicibank.com/EazyPGVerify
    ?ezpaytranid=&amount=&paymentmode=
    &merchantid=380738&trandate=&pgreferenceno={refno}&dstatus=Y

One script, two Windows Task Scheduler jobs (do not copy this file):

  1) Every 5 minutes, last 5 minutes of unpaid rows:
       python eazypay_verify_job.py --once

  2) Every day at 23:00 (11:00 PM), last 24 hours of unpaid rows:
       python eazypay_verify_job.py --daily

Call Verify for each refno and update the same columns ReturnUrl updates:
  TRANSACTIONAMOUNT, UNIQUEREFNUMBER, TRANSACTIONDATE, BANKSTATUS
on ONLINE_ROLL_AFF and BANK_RETURN_LOG_AFF.

EAZYPAY_VERIFY_JOB has two rows:
  JOB_ID=1 EAZYPAY_VERIFY_5MIN     --once / loop
  JOB_ID=2 EAZYPAY_VERIFY_DAILY24H --daily at 23:00

Loop in this process (5-minute lookback):
  python eazypay_verify_job.py
"""
from __future__ import print_function

import argparse
import os
import sys
import time
import traceback
from datetime import datetime, timedelta

try:
    from urllib.parse import parse_qs, urlencode
    from urllib.request import urlopen, Request
except ImportError:
    from urllib import urlencode
    from urllib2 import urlopen, Request
    from urlparse import parse_qs

try:
    import oracledb
except ImportError:
    oracledb = None
    try:
        import cx_Oracle as oracledb
    except ImportError:
        oracledb = None

MERCHANT_ID = os.environ.get("EAZYPAY_MERCHANT_ID", "380738")
VERIFY_URL = os.environ.get(
    "EAZYPAY_VERIFY_URL", "https://eazypay.icicibank.com/EazyPGVerify"
)
ORACLE_USER = os.environ.get("ORACLE_USER", "college")
ORACLE_PASSWORD = os.environ.get("ORACLE_PASSWORD", "college")
ORACLE_DSN = os.environ.get("ORACLE_DSN", "66.85.138.226:1521/coepdb")
FEE_UID = os.environ.get("EAZYPAY_FEE_UID", "AFFILIATED(EXAM FEE)")
LOOKBACK_MINUTES = int(os.environ.get("LOOKBACK_MINUTES", "5"))
INTERVAL_SECONDS = int(os.environ.get("INTERVAL_SECONDS", "300"))
HTTP_TIMEOUT = int(os.environ.get("HTTP_TIMEOUT", "30"))

SUCCESS_STATUSES = set(["success", "rip", "sip"])
FAIL_STATUSES = set([
    "failed", "timeout", "transaction expired", "user dropped",
    "cheque/dd returned", "canceled by user", "cancelled by user",
])
PENDING_STATUSES = set([
    "transaction initiated", "challan generated", "cheque/dd in clearance",
])

JOB_5MIN = 1
JOB_DAILY = 2
JOB_NAMES = {
    JOB_5MIN: "EAZYPAY_VERIFY_5MIN",
    JOB_DAILY: "EAZYPAY_VERIFY_DAILY24H",
}


def log(msg):
    print(datetime.now().strftime("%Y-%m-%d %H:%M:%S") + " " + msg)
    sys.stdout.flush()


def connect():
    if oracledb is None:
        raise RuntimeError("Install oracledb: pip install oracledb")
    return oracledb.connect(user=ORACLE_USER, password=ORACLE_PASSWORD, dsn=ORACLE_DSN)


def verify_refno(refno):
    query = urlencode({
        "ezpaytranid": "",
        "amount": "",
        "paymentmode": "",
        "merchantid": MERCHANT_ID,
        "trandate": "",
        "pgreferenceno": refno,
        "dstatus": "Y",
    })
    url = VERIFY_URL + "?" + query
    req = Request(url)
    req.add_header("User-Agent", "AKV-ExamEnq-EazyPGVerify/1.0")
    resp = urlopen(req, timeout=HTTP_TIMEOUT)
    raw = resp.read()
    if isinstance(raw, bytes):
        raw = raw.decode("utf-8", "replace")
    raw = (raw or "").strip()
    parsed = {}
    for key, values in parse_qs(raw, keep_blank_values=True).items():
        parsed[key.lower()] = values[0] if values else ""
    if "status" not in parsed and "=" not in raw:
        parsed["status"] = raw
        parsed["_raw"] = raw
    else:
        parsed["_raw"] = raw
    return parsed


def classify(status):
    s = (status or "").strip().lower()
    if s in SUCCESS_STATUSES:
        return "Success"
    if s in FAIL_STATUSES:
        return "Declined"
    if s in PENDING_STATUSES or not s:
        return None
    if "success" in s:
        return "Success"
    if "fail" in s or "timeout" in s or "expir" in s:
        return "Declined"
    return None


def ensure_job_rows(conn):
    cur = conn.cursor()
    cur.execute(
        """
        MERGE INTO EAZYPAY_VERIFY_JOB t
        USING (
            SELECT 1 AS JOB_ID, 'EAZYPAY_VERIFY_5MIN' AS JOB_NAME FROM dual
            UNION ALL
            SELECT 2, 'EAZYPAY_VERIFY_DAILY24H' FROM dual
        ) s
        ON (t.JOB_ID = s.JOB_ID)
        WHEN MATCHED THEN
            UPDATE SET t.JOB_NAME = s.JOB_NAME
        WHEN NOT MATCHED THEN
            INSERT (JOB_ID, JOB_NAME, LAST_STATUS, LAST_NOTE)
            VALUES (s.JOB_ID, s.JOB_NAME, 'NEW', 'Waiting for first run')
        """
    )
    conn.commit()
    cur.close()


def mark_job_start(conn, job_id, window_from, window_to):
    cur = conn.cursor()
    cur.execute(
        """
        UPDATE EAZYPAY_VERIFY_JOB
           SET LAST_START_TS = SYSTIMESTAMP,
               LAST_STATUS = 'RUNNING',
               WINDOW_FROM = :wf,
               WINDOW_TO = :wt,
               LAST_ERROR = NULL
         WHERE JOB_ID = :pjid
        """,
        wf=window_from,
        wt=window_to,
        pjid=job_id,
    )
    conn.commit()
    cur.close()


def mark_job_end(conn, job_id, status, found, checked, updated, error, note):
    cur = conn.cursor()
    cur.execute(
        """
        UPDATE EAZYPAY_VERIFY_JOB
           SET LAST_END_TS = SYSTIMESTAMP,
               LAST_STATUS = :pstatus,
               ROWS_FOUND = :found,
               ROWS_CHECKED = :checked,
               ROWS_UPDATED = :updated,
               LAST_ERROR = :err,
               LAST_NOTE = :note
         WHERE JOB_ID = :pjid
        """,
        pstatus=status,
        found=found,
        checked=checked,
        updated=updated,
        err=(error or "")[:2000] or None,
        note=(note or "")[:1000] or None,
        pjid=job_id,
    )
    conn.commit()
    cur.close()


def load_pending(conn, window_from, window_to, all_unpaid=False):
    cur = conn.cursor()
    if all_unpaid:
        cur.execute(
            """
            SELECT REFNO, PGAMOUNT, BANKSTATUS, DT
              FROM ONLINE_ROLL_AFF
             WHERE (BANKSTATUS IS NULL OR UPPER(TRIM(BANKSTATUS)) <> 'SUCCESS')
             ORDER BY DT
            """
        )
    else:
        cur.execute(
            """
            SELECT REFNO, PGAMOUNT, BANKSTATUS, DT
              FROM ONLINE_ROLL_AFF
             WHERE DT >= :wf
               AND DT < :wt
               AND FEEUID = :pfeeuid
               AND (BANKSTATUS IS NULL OR UPPER(TRIM(BANKSTATUS)) <> 'SUCCESS')
             ORDER BY DT
            """,
            wf=window_from,
            wt=window_to,
            pfeeuid=FEE_UID,
        )
    rows = cur.fetchall()
    cur.close()
    return rows


def update_bank_status(conn, refno, amount, unique_ref, txn_date, bank_status):
    cur = conn.cursor()
    for table in ("ONLINE_ROLL_AFF", "BANK_RETURN_LOG_AFF"):
        cur.execute(
            """
            UPDATE """ + table + """
               SET TRANSACTIONAMOUNT = :pamt,
                   UNIQUEREFNUMBER = :puref,
                   TRANSACTIONDATE = :ptdate,
                   BANKSTATUS = :pstatus
             WHERE REFNO = :prefno
               AND (
                     :pstatus = 'Success'
                  OR BANKSTATUS IS NULL
                  OR UPPER(TRIM(BANKSTATUS)) <> 'SUCCESS'
               )
            """,
            pamt=amount,
            puref=unique_ref,
            ptdate=txn_date,
            pstatus=bank_status,
            prefno=refno,
        )
    conn.commit()
    cur.close()


def parse_amount(parsed):
    raw = parsed.get("ba") or parsed.get("amount") or ""
    raw = str(raw).strip()
    if not raw:
        return None
    try:
        return float(raw)
    except ValueError:
        return None


def run_once(all_unpaid=False, lookback_minutes=None, mode=None, job_id=JOB_5MIN):
    if lookback_minutes is None:
        lookback_minutes = LOOKBACK_MINUTES
    if not mode:
        mode = "all" if all_unpaid else ("%sm" % lookback_minutes)
    window_to = datetime.now()
    window_from = datetime(2000, 1, 1) if all_unpaid else window_to - timedelta(minutes=lookback_minutes)
    found = checked = updated = 0
    conn = connect()
    try:
        ensure_job_rows(conn)
        mark_job_start(conn, job_id, window_from, window_to)
        rows = load_pending(conn, window_from, window_to, all_unpaid=all_unpaid)
        found = len(rows)
        log("job_id=%s mode=%s window %s -> %s all=%s found=%s" % (
            job_id, mode, window_from, window_to, all_unpaid, found
        ))
        notes = []
        for refno, pgamount, bankstatus, dt in rows:
            checked += 1
            try:
                parsed = verify_refno(refno)
            except Exception as ex:
                notes.append("%s verify-error:%s" % (refno, ex))
                log("verify error %s %s" % (refno, ex))
                continue
            status = parsed.get("status", "")
            mapped = classify(status)
            log("refno=%s dt=%s bank=%s verify=%s mapped=%s raw=%s" % (
                refno, dt, bankstatus, status, mapped, parsed.get("_raw", "")[:200]
            ))
            if mapped is None:
                notes.append("%s pending:%s" % (refno, status or "empty"))
                continue
            amount = parse_amount(parsed)
            if amount is None:
                amount = pgamount
            unique_ref = parsed.get("ezpaytranid") or None
            txn_date = parsed.get("trandate") or None
            update_bank_status(conn, refno, amount, unique_ref, txn_date, mapped)
            updated += 1
            notes.append("%s -> %s" % (refno, mapped))
        note = "mode=%s; " % mode + "; ".join(notes[-8:])
        mark_job_end(conn, job_id, "OK", found, checked, updated, None, note)
        log("done job_id=%s mode=%s found=%s checked=%s updated=%s" % (
            job_id, mode, found, checked, updated
        ))
        return 0
    except Exception:
        err = traceback.format_exc()
        log(err)
        try:
            mark_job_end(conn, job_id, "ERROR", found, checked, updated, err, None)
        except Exception:
            pass
        return 1
    finally:
        try:
            conn.close()
        except Exception:
            pass


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--once", action="store_true", help="one pass, last 5 minutes (Task Scheduler)")
    parser.add_argument(
        "--daily",
        action="store_true",
        help="one pass, last 24 hours of unpaid rows (Task Scheduler at 23:00 / 11:00 PM)",
    )
    parser.add_argument(
        "--lookback-minutes",
        type=int,
        default=None,
        help="override lookback window in minutes",
    )
    parser.add_argument(
        "--all",
        action="store_true",
        help="check every unpaid ONLINE_ROLL_AFF row (testing)",
    )
    args = parser.parse_args()
    lookback = args.lookback_minutes
    mode = None
    if args.all:
        mode = "all"
        lookback = lookback if lookback is not None else LOOKBACK_MINUTES
        sys.exit(run_once(all_unpaid=True, lookback_minutes=lookback, mode=mode, job_id=JOB_5MIN))
    if args.daily:
        lookback = 24 * 60 if lookback is None else lookback
        sys.exit(run_once(all_unpaid=False, lookback_minutes=lookback, mode="daily24h", job_id=JOB_DAILY))
    if args.once:
        lookback = LOOKBACK_MINUTES if lookback is None else lookback
        sys.exit(run_once(all_unpaid=False, lookback_minutes=lookback, mode="%sm" % lookback, job_id=JOB_5MIN))
    log("loop every %s seconds, lookback %s minutes" % (INTERVAL_SECONDS, LOOKBACK_MINUTES))
    while True:
        run_once(lookback_minutes=LOOKBACK_MINUTES, mode="%sm" % LOOKBACK_MINUTES, job_id=JOB_5MIN)
        time.sleep(INTERVAL_SECONDS)


if __name__ == "__main__":
    main()
