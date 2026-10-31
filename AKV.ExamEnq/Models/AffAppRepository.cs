using AKV.ExamEnq.Report;
using AKV.ExanEnq.Entity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Oracle.ManagedDataAccess.Client;

namespace AKV.ExamEnq.Models
{
    public class AffAppStudentRow
    {
        public string coursecode { get; set; }
        public string coursename { get; set; }
        public string collegecode { get; set; }
        public string collegename { get; set; }
        public string sem_no { get; set; }
        public string name { get; set; }
        public string regno { get; set; }
        public string rollno { get; set; }
        public decimal demand { get; set; }
        public byte[] photo { get; set; }
        public string id { get; set; }
    }

    public class AffAppPaperRow
    {
        public string sem_no { get; set; }
        public string pap_no { get; set; }
        public string papname { get; set; }
        public int papfee { get; set; }
    }

    public class AffAppPaymentRow
    {
        public string refno { get; set; }
        public string collegecode { get; set; }
        public string collegename { get; set; }
        public string coursecode { get; set; }
        public string coursename { get; set; }
        public string semester { get; set; }
        public string regno { get; set; }
        public string stname { get; set; }
        public string papers { get; set; }
        public decimal? transactionamount { get; set; }
        public string transactiondate { get; set; }
        public string uniquerefnumber { get; set; }
        public string bankstatus { get; set; }
        public decimal? pgamount { get; set; }
    }

    public static class AffAppRepository
    {
        private static string ConnectionString
        {
            get
            {
                var cs = ConfigurationManager.ConnectionStrings["college"];
                if (cs == null || string.IsNullOrWhiteSpace(cs.ConnectionString))
                {
                    throw new InvalidOperationException("Oracle connection string 'college' is missing.");
                }
                return cs.ConnectionString;
            }
        }

        public static AffAppStatusVm GetAppStatus()
        {
            var mthyr = CurrentMthyr();
            var status = new AffAppStatusVm { mthyr = mthyr };
            using (var con = new OracleConnection(ConnectionString))
            {
                con.Open();
                status.rollWithPapers = Count(con,
                    "SELECT COUNT(*) FROM ROLL WHERE PAPERS IS NOT NULL");
                status.affYearSuccessAmount = Amount(con,
                    @"SELECT NVL(SUM(NVL(TRANSACTIONAMOUNT, 0)), 0) FROM ONLINE_ROLL_AFF
                       WHERE BANKSTATUS = 'Success'
                         AND SUBSTR(REFNO, 1, 3) = :pmthyr
                         AND FEEUID = :puid",
                    mthyr);
                status.affYearSuccess = Count(con,
                    @"SELECT COUNT(*) FROM ONLINE_ROLL_AFF
                       WHERE BANKSTATUS = 'Success'
                         AND SUBSTR(REFNO, 1, 3) = :pmthyr
                         AND FEEUID = :puid",
                    mthyr);
                status.hallTicketPaid = Count(con,
                    "SELECT COUNT(*) FROM ROLL WHERE NVL(PAID, 0) > 0");
            }
            return status;
        }

        private static int Count(OracleConnection con, string sql, string mthyr = null)
        {
            using (var cmd = con.CreateCommand())
            {
                cmd.BindByName = true;
                cmd.CommandText = sql;
                if (mthyr != null)
                {
                    cmd.Parameters.Add("pmthyr", OracleDbType.Varchar2).Value = mthyr;
                    cmd.Parameters.Add("puid", OracleDbType.Varchar2).Value = EazypayService.Uid;
                }
                var value = cmd.ExecuteScalar();
                return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
            }
        }

        private static decimal Amount(OracleConnection con, string sql, string mthyr)
        {
            using (var cmd = con.CreateCommand())
            {
                cmd.BindByName = true;
                cmd.CommandText = sql;
                cmd.Parameters.Add("pmthyr", OracleDbType.Varchar2).Value = mthyr;
                cmd.Parameters.Add("puid", OracleDbType.Varchar2).Value = EazypayService.Uid;
                var value = cmd.ExecuteScalar();
                return value == null || value == DBNull.Value ? 0 : Convert.ToDecimal(value);
            }
        }

        public static AffAppStudentRow GetStudent(string regno)
        {
            if (string.IsNullOrWhiteSpace(regno)) return null;
            using (var et = new Entities())
            {
                return et.Database.SqlQuery<AffAppStudentRow>(
                    "select v.coursecode,v.coursename,v.collegecode,v.collegename,v.sem_no,v.regno,v.rollno,v.name,nvl(v.demand,0) demand,v.photo," +
                    "(select r.id from roll r where (r.regno = v.regno or r.rollno = v.rollno) and rownum = 1) id " +
                    "from view_api_roll v where (v.rollno = '" + regno + "' or v.regno= '" + regno + "')").FirstOrDefault();
            }
        }

        public static List<AffAppPaperRow> GetPapers(string regno)
        {
            if (string.IsNullOrWhiteSpace(regno)) return new List<AffAppPaperRow>();
            using (var et = new Entities())
            {
                return et.Database.SqlQuery<AffAppPaperRow>(
                    "select sem_no,pap_no,papname,nvl(papfee,0) papfee from view_api_papers where (rollno = '" + regno + "' or regno= '" + regno + "')").ToList();
            }
        }

        public static List<AffAppPaymentRow> GetSuccessPayments(string regno)
        {
            using (var con = new OracleConnection(ConnectionString))
            using (var cmd = con.CreateCommand())
            {
                con.Open();
                cmd.BindByName = true;
                cmd.CommandText = @"SELECT REFNO, COLLEGECODE, COLLEGENAME, COURSECODE, COURSENAME, SEMESTER,
                    REGNO, STNAME, PAPERS, TRANSACTIONAMOUNT, TRANSACTIONDATE, UNIQUEREFNUMBER, BANKSTATUS, PGAMOUNT
                    FROM ONLINE_ROLL_AFF
                    WHERE BANKSTATUS = 'Success' AND FEEUID = :puid AND REGNO = :pregno
                      AND SUBSTR(REFNO, 1, 3) = :pmthyr
                    ORDER BY REFNO";
                cmd.Parameters.Add("puid", OracleDbType.Varchar2).Value = EazypayService.Uid;
                cmd.Parameters.Add("pregno", OracleDbType.Varchar2).Value = regno;
                cmd.Parameters.Add("pmthyr", OracleDbType.Varchar2).Value = CurrentMthyr();
                return ReadPayments(cmd);
            }
        }

        public static AffAppPaymentRow GetSuccessByRefno(string refno)
        {
            using (var con = new OracleConnection(ConnectionString))
            using (var cmd = con.CreateCommand())
            {
                con.Open();
                cmd.BindByName = true;
                cmd.CommandText = @"SELECT REFNO, COLLEGECODE, COLLEGENAME, COURSECODE, COURSENAME, SEMESTER,
                    REGNO, STNAME, PAPERS, TRANSACTIONAMOUNT, TRANSACTIONDATE, UNIQUEREFNUMBER, BANKSTATUS, PGAMOUNT
                    FROM ONLINE_ROLL_AFF
                    WHERE REFNO = :prefno AND BANKSTATUS = 'Success' AND FEEUID = :puid
                      AND SUBSTR(REFNO, 1, 3) = :pmthyr";
                cmd.Parameters.Add("prefno", OracleDbType.Varchar2).Value = WithYearPrefix(refno);
                cmd.Parameters.Add("pmthyr", OracleDbType.Varchar2).Value = CurrentMthyr();
                cmd.Parameters.Add("puid", OracleDbType.Varchar2).Value = EazypayService.Uid;
                return ReadPayments(cmd).FirstOrDefault();
            }
        }

        public static AffAppPaymentRow GetByRefno(string refno)
        {
            if (string.IsNullOrWhiteSpace(refno)
                || string.Equals(refno, "null", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            var row = GetByRefnoExact(refno);
            if (row != null)
            {
                return row;
            }
            var prefixed = WithYearPrefix(refno);
            if (!string.Equals(prefixed, refno, StringComparison.OrdinalIgnoreCase))
            {
                return GetByRefnoExact(prefixed);
            }
            return null;
        }

        private static AffAppPaymentRow GetByRefnoExact(string refno)
        {
            using (var con = new OracleConnection(ConnectionString))
            using (var cmd = con.CreateCommand())
            {
                con.Open();
                cmd.BindByName = true;
                cmd.CommandText = @"SELECT REFNO, COLLEGECODE, COLLEGENAME, COURSECODE, COURSENAME, SEMESTER,
                    REGNO, STNAME, PAPERS, TRANSACTIONAMOUNT, TRANSACTIONDATE, UNIQUEREFNUMBER, BANKSTATUS, PGAMOUNT
                    FROM ONLINE_ROLL_AFF WHERE REFNO = :prefno";
                cmd.Parameters.Add("prefno", OracleDbType.Varchar2).Value = refno;
                return ReadPayments(cmd).FirstOrDefault();
            }
        }

        public static AffAppYearVm GetYearSettings()
        {
            using (var con = new OracleConnection(ConnectionString))
            using (var cmd = con.CreateCommand())
            {
                con.Open();
                cmd.CommandText =
                    "SELECT TRIM(MTHYR), NVL(SHOWPAYMENT, 0), NVL(FINE, 0) FROM EAZYPAY_YEAROFEXAM WHERE ROWNUM = 1";
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read())
                    {
                        throw new InvalidOperationException("EAZYPAY_YEAROFEXAM has no row.");
                    }
                    var mthyr = Convert.ToString(r.GetValue(0));
                    if (string.IsNullOrWhiteSpace(mthyr) || mthyr.Trim().Length != 3)
                    {
                        throw new InvalidOperationException("EAZYPAY_YEAROFEXAM.MTHYR must be exactly 3 characters.");
                    }
                    return new AffAppYearVm
                    {
                        mthyr = mthyr.Trim().ToUpperInvariant(),
                        showPayment = IsShowPaymentOn(r.GetValue(1)),
                        fine = Convert.ToInt32(Convert.ToDecimal(r.GetValue(2)))
                    };
                }
            }
        }

        private static bool IsShowPaymentOn(object value)
        {
            if (value == null || value == DBNull.Value) return false;
            if (value is bool b) return b;
            try
            {
                return Convert.ToInt32(Convert.ToDecimal(value)) == 1;
            }
            catch
            {
                var text = Convert.ToString(value).Trim();
                return string.Equals(text, "1", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(text, "Y", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(text, "TRUE", StringComparison.OrdinalIgnoreCase);
            }
        }

        public static string CurrentMthyr()
        {
            return GetYearSettings().mthyr;
        }

        public static string WithYearPrefix(string refno)
        {
            if (string.IsNullOrWhiteSpace(refno)
                || string.Equals(refno, "null", StringComparison.OrdinalIgnoreCase))
            {
                return refno;
            }
            var prefix = CurrentMthyr();
            if (refno.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return refno;
            }
            return prefix + refno;
        }

        public static string NextBankUid()
        {
            var prefix = CurrentMthyr();
            using (var con = new OracleConnection(ConnectionString))
            using (var cmd = con.CreateCommand())
            {
                con.Open();
                cmd.CommandText = "SELECT SEQ_ONLINE_ROLL_AFF.NEXTVAL FROM DUAL";
                var value = Convert.ToInt64(cmd.ExecuteScalar());
                if (value < 1000000000L || value > 9999999999L)
                {
                    throw new InvalidOperationException("ONLINE_ROLL_AFF sequence must be exactly 10 digits.");
                }
                var digits = value.ToString("0000000000");
                if (digits.Length != 10)
                {
                    throw new InvalidOperationException("ONLINE_ROLL_AFF reference must be exactly 10 digits.");
                }
                return prefix + digits;
            }
        }

        public static void InsertPayment(AffAppPaymentInsert row)
        {
            row.fine = GetYearSettings().fine;
            using (var con = new OracleConnection(ConnectionString))
            {
                con.Open();
                using (var tx = con.BeginTransaction())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.BindByName = true;
                        cmd.Transaction = tx;
                        cmd.CommandText = @"INSERT INTO ONLINE_ROLL_AFF
                            (REFNO, SUBMERCHANTID, PGAMOUNT, STNAME, EMAIL, MOBILENO, REGNO, ROLLNO, FEEUID, FCODE,
                             SEMESTER, COURSECODE, FEEMODE, TRANSACTIONAMOUNT, UNIQUEREFNUMBER, TRANSACTIONDATE, BANKSTATUS,
                             PAPERS, FACULTY, TOKEN, PROVFEES, DEGFEES, CONFEES, COURSENAME, COLLEGECODE, COLLEGENAME, FINE, ID)
                            VALUES
                            (:prefno, :psubmerchantid, :ppgamount, :pstname, :pemail, :pmobileno, :pregno, :prollno, :puid, :pfcode,
                             :psemester, :pcoursecode, :pmode, NULL, NULL, NULL, NULL,
                             :ppapers, :pfaculty, :ptoken, :pprovfees, :pdegfees, :pconfees, :pcoursename, :pcollegecode, :pcollegename, :pfine, :pid)";
                        BindInsert(cmd, row);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = con.CreateCommand())
                    {
                        cmd.BindByName = true;
                        cmd.Transaction = tx;
                        cmd.CommandText = @"INSERT INTO BANK_RETURN_LOG_AFF
                            (REFNO, SUBMERCHANTID, PGAMOUNT, STNAME, EMAIL, MOBILENO, REGNO, ROLLNO, FEEUID, FCODE,
                             SEMESTER, COURSECODE, FEEMODE, TRANSACTIONAMOUNT, UNIQUEREFNUMBER, TRANSACTIONDATE, BANKSTATUS,
                             PAPERS, FACULTY, TOKEN, PROVFEES, DEGFEES, CONFEES, COURSENAME, COLLEGECODE, COLLEGENAME, FINE, PROGRAMME)
                            VALUES
                            (:prefno, :psubmerchantid, :ppgamount, :pstname, :pemail, :pmobileno, :pregno, :prollno, :puid, :pfcode,
                             :psemester, :pcoursecode, :pmode, NULL, NULL, NULL, NULL,
                             :ppapers, :pfaculty, :ptoken, :pprovfees, :pdegfees, :pconfees, :pcoursename, :pcollegecode, :pcollegename, :pfine, ' ')";
                        BindInsert(cmd, row);
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
            }
        }

        public static void UpdateBankStatus(string refno, decimal? transactionAmount, string uniqueRefNumber, string transactionDate, string bankStatus)
        {
            var existing = GetByRefno(refno);
            var key = existing != null ? existing.refno : WithYearPrefix(refno);
            using (var con = new OracleConnection(ConnectionString))
            {
                con.Open();
                using (var tx = con.BeginTransaction())
                {
                    foreach (var table in new[] { "ONLINE_ROLL_AFF", "BANK_RETURN_LOG_AFF" })
                    {
                        using (var cmd = con.CreateCommand())
                        {
                            cmd.BindByName = true;
                            cmd.Transaction = tx;
                            cmd.CommandText = "UPDATE " + table + @" SET
                                TRANSACTIONAMOUNT = :pamt,
                                UNIQUEREFNUMBER = :puref,
                                TRANSACTIONDATE = :ptdate,
                                BANKSTATUS = :pstatus
                                WHERE REFNO = :prefno
                                  AND (
                                        :pstatus = 'Success'
                                     OR BANKSTATUS IS NULL
                                     OR UPPER(TRIM(BANKSTATUS)) <> 'SUCCESS'
                                  )";
                            cmd.Parameters.Add("pamt", OracleDbType.Decimal).Value = (object)transactionAmount ?? DBNull.Value;
                            cmd.Parameters.Add("puref", OracleDbType.Varchar2).Value = (object)uniqueRefNumber ?? DBNull.Value;
                            cmd.Parameters.Add("ptdate", OracleDbType.Varchar2).Value = (object)transactionDate ?? DBNull.Value;
                            cmd.Parameters.Add("pstatus", OracleDbType.Varchar2).Value = (object)bankStatus ?? DBNull.Value;
                            cmd.Parameters.Add("prefno", OracleDbType.Varchar2).Value = key;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                }
            }
        }

        public static AffAppReportPoco ToReport(AffAppPaymentRow row)
        {
            if (row == null) return null;
            var papers = (row.papers ?? string.Empty).Trim('|').Replace("|", ", ");
            return new AffAppReportPoco
            {
                collegecode = row.collegecode,
                collegename = row.collegename,
                coursecode = row.coursecode,
                coursename = row.coursename,
                regno = row.regno,
                rollno = row.regno,
                sem_no = row.semester == "9" ? "Private" : row.semester,
                name = row.stname,
                stname = row.stname,
                papers = papers,
                examfee = row.transactionamount ?? 0,
                transactionamount = Convert.ToString(row.transactionamount ?? 0),
                uniquerefnumber = row.uniquerefnumber,
                transactiondate = row.transactiondate,
                bankstatus = row.bankstatus,
                refno = row.refno,
                gateway = "ICICI BANK",
                reportdate = DateTime.Now.ToString("dd/MM/yyyy"),
                semesterdisplay = row.semester == "9" ? "Private" : row.semester
            };
        }

        private static void BindInsert(OracleCommand cmd, AffAppPaymentInsert row)
        {
            cmd.Parameters.Add("prefno", OracleDbType.Varchar2).Value = row.refno;
            cmd.Parameters.Add("psubmerchantid", OracleDbType.Varchar2).Value = row.submerchantid;
            cmd.Parameters.Add("ppgamount", OracleDbType.Decimal).Value = row.pgamount;
            cmd.Parameters.Add("pstname", OracleDbType.Varchar2).Value = row.stname;
            cmd.Parameters.Add("pemail", OracleDbType.Varchar2).Value = row.email;
            cmd.Parameters.Add("pmobileno", OracleDbType.Varchar2).Value = row.mobileno;
            cmd.Parameters.Add("pregno", OracleDbType.Varchar2).Value = row.regno;
            cmd.Parameters.Add("prollno", OracleDbType.Varchar2).Value = row.rollno ?? string.Empty;
            cmd.Parameters.Add("puid", OracleDbType.Varchar2).Value = row.uid;
            cmd.Parameters.Add("pfcode", OracleDbType.Varchar2).Value = row.fcode;
            cmd.Parameters.Add("psemester", OracleDbType.Varchar2).Value = (object)row.semester ?? DBNull.Value;
            cmd.Parameters.Add("pcoursecode", OracleDbType.Varchar2).Value = row.coursecode;
            cmd.Parameters.Add("pmode", OracleDbType.Varchar2).Value = row.mode;
            cmd.Parameters.Add("ppapers", OracleDbType.Varchar2).Value = (object)row.papers ?? DBNull.Value;
            cmd.Parameters.Add("pfaculty", OracleDbType.Varchar2).Value = row.faculty ?? string.Empty;
            cmd.Parameters.Add("ptoken", OracleDbType.Varchar2).Value = row.token ?? string.Empty;
            cmd.Parameters.Add("pprovfees", OracleDbType.Decimal).Value = row.provfees;
            cmd.Parameters.Add("pdegfees", OracleDbType.Decimal).Value = row.degfees;
            cmd.Parameters.Add("pconfees", OracleDbType.Decimal).Value = row.confees;
            cmd.Parameters.Add("pcoursename", OracleDbType.Varchar2).Value = row.coursename;
            cmd.Parameters.Add("pcollegecode", OracleDbType.Varchar2).Value = row.collegecode;
            cmd.Parameters.Add("pcollegename", OracleDbType.Varchar2).Value = row.collegename;
            cmd.Parameters.Add("pfine", OracleDbType.Decimal).Value = row.fine;
            cmd.Parameters.Add("pid", OracleDbType.Varchar2).Value = (object)row.id ?? DBNull.Value;
        }

        private static List<AffAppPaymentRow> ReadPayments(OracleCommand cmd)
        {
            var list = new List<AffAppPaymentRow>();
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    list.Add(new AffAppPaymentRow
                    {
                        refno = Convert.ToString(r["REFNO"]),
                        collegecode = Convert.ToString(r["COLLEGECODE"]),
                        collegename = Convert.ToString(r["COLLEGENAME"]),
                        coursecode = Convert.ToString(r["COURSECODE"]),
                        coursename = Convert.ToString(r["COURSENAME"]),
                        semester = Convert.ToString(r["SEMESTER"]),
                        regno = Convert.ToString(r["REGNO"]),
                        stname = Convert.ToString(r["STNAME"]),
                        papers = Convert.ToString(r["PAPERS"]),
                        transactionamount = r["TRANSACTIONAMOUNT"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["TRANSACTIONAMOUNT"]),
                        transactiondate = Convert.ToString(r["TRANSACTIONDATE"]),
                        uniquerefnumber = Convert.ToString(r["UNIQUEREFNUMBER"]),
                        bankstatus = Convert.ToString(r["BANKSTATUS"]),
                        pgamount = r["PGAMOUNT"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["PGAMOUNT"])
                    });
                }
            }
            return list;
        }
    }

    public class AffAppPaymentInsert
    {
        public string refno { get; set; }
        public string submerchantid { get; set; }
        public decimal pgamount { get; set; }
        public string stname { get; set; }
        public string email { get; set; }
        public string mobileno { get; set; }
        public string regno { get; set; }
        public string rollno { get; set; }
        public string uid { get; set; }
        public string fcode { get; set; }
        public string semester { get; set; }
        public string coursecode { get; set; }
        public string mode { get; set; }
        public string papers { get; set; }
        public string faculty { get; set; }
        public string token { get; set; }
        public decimal provfees { get; set; }
        public decimal degfees { get; set; }
        public decimal confees { get; set; }
        public string coursename { get; set; }
        public string collegecode { get; set; }
        public string collegename { get; set; }
        public decimal fine { get; set; }
        public string id { get; set; }
    }
}
