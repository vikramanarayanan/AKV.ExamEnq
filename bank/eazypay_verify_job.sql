-- Heartbeat rows for the EazyPGVerify jobs.
-- JOB_ID=1 every 5 minutes (last 5 minutes)
-- JOB_ID=2 every day at 23:00 (last 24 hours)
-- Safe to re-run: does not drop existing last-run timestamps.

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
    VALUES (s.JOB_ID, s.JOB_NAME, 'NEW', 'Waiting for first run');

COMMIT;

BEGIN
   EXECUTE IMMEDIATE 'CREATE INDEX IX_ONLINE_ROLL_AFF_DT ON ONLINE_ROLL_AFF (DT)';
EXCEPTION
   WHEN OTHERS THEN
      IF SQLCODE != -955 THEN RAISE; END IF;
END;
/

EXIT;
