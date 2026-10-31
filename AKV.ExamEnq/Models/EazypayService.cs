using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace AKV.ExamEnq.Models
{
    /// <summary>
    /// ICICI eazypay integration matching bank/useraff.php and bank/affreturnurl.php.
    /// </summary>
    public static class EazypayService
    {
        public static string MerchantId { get { return Setting("Eazypay.MerchantId", "380738"); } }
        public static string AesKey { get { return Setting("Eazypay.AesKey", "1162121111101159"); } }
        public static string SubMerchantId { get { return Setting("Eazypay.SubMerchantId", "55"); } }
        public static string PayMode { get { return Setting("Eazypay.PayMode", "9"); } }
        public static string ReturnUrl { get { return Setting("Eazypay.ReturnUrl", "https://aucoeexam.in/bank/returnurl"); } }
        public static string PaymentUrl { get { return Setting("Eazypay.PaymentUrl", "https://eazypay.icicibank.com/EazyPG"); } }
        public static string VerifyUrl { get { return Setting("Eazypay.VerifyUrl", "https://eazypay.icicibank.com/EazyPGVerify"); } }
        public static string Fcode { get { return Setting("Eazypay.Fcode", "7151"); } }
        public static string Uid { get { return "AFFILIATED(EXAM FEE)"; } }
        public const string RefCookieName = "AffPayRefno";
        public const string ReceiptPageSessionKey = "AffShowReceipt";

        public static void AllowReceiptPage(HttpSessionStateBase session)
        {
            if (session != null)
            {
                session[ReceiptPageSessionKey] = "1";
            }
        }

        public static bool CanShowReceiptPage(HttpSessionStateBase session)
        {
            return session != null
                && string.Equals(Convert.ToString(session[ReceiptPageSessionKey]), "1", StringComparison.Ordinal);
        }

        public class GatewayForm
        {
            public string ActionUrl { get; set; }
            public Dictionary<string, string> Fields { get; set; }
        }

        public class GatewayResponse
        {
            public string ResponseCode { get; set; }
            public string UniqueRefNumber { get; set; }
            public string ServiceTaxAmount { get; set; }
            public string ProcessingFeeAmount { get; set; }
            public string TotalAmount { get; set; }
            public string TransactionAmount { get; set; }
            public string TransactionDate { get; set; }
            public string InterchangeValue { get; set; }
            public string Tdr { get; set; }
            public string PaymentMode { get; set; }
            public string SubMerchantId { get; set; }
            public string ReferenceNo { get; set; }
            public string Tps { get; set; }
            public string MerchantId { get; set; }
            public string Rs { get; set; }
            public string MandatoryFields { get; set; }
            public bool HashValid { get; set; }
            public bool IsSuccess { get; set; }
            public string BankStatus { get; set; }
            public string StudentName { get; set; }
            public string Regno { get; set; }
            public string Mobile { get; set; }
            public string Email { get; set; }
            public string Uid { get; set; }
        }

        public class VerifyResult
        {
            public string Status { get; set; }
            public string EzpayTranId { get; set; }
            public string Amount { get; set; }
            public string TranDate { get; set; }
            public string Raw { get; set; }
        }

        public class PaymentConfirm
        {
            public string BankStatus { get; set; }
            public decimal? Amount { get; set; }
            public string UniqueRef { get; set; }
            public string TransactionDate { get; set; }
            public bool ApplyUpdate { get; set; }
        }

        public static string BuildPhpMandatory(string refno, string amount, string studentName, string regno, string mobile, string email)
        {
            // useraff.php: $refno.'|'.$submerchantid.'|'.$pgamount.'|'.$stname.'|'.$regno.'|'.$uid.'|'.$fcode.'|'.$mob.'|'.$email
            return Clean(refno) + "|" + Clean(SubMerchantId) + "|" + Clean(amount) + "|"
                + Clean(studentName) + "|" + Clean(regno) + "|" + Clean(Uid) + "|"
                + Clean(Fcode) + "|" + Clean(mobile) + "|" + Clean(email);
        }

        public static GatewayForm BuildPaymentForm(string refno, decimal amount, string studentName, string regno, string mobile, string email)
        {
            var amt = FormatAmount(amount);
            var mandatory = BuildPhpMandatory(refno, amt, studentName, regno, mobile, email);
            var fields = new Dictionary<string, string>(StringComparer.Ordinal);
            fields.Add("merchantid", MerchantId);
            fields.Add("mandatory fields", Encrypt(mandatory));
            fields.Add("optional fields", string.Empty);
            fields.Add("returnurl", Encrypt(ReturnUrl));
            fields.Add("Reference No", Encrypt(Clean(refno)));
            fields.Add("submerchantid", Encrypt(SubMerchantId));
            fields.Add("transaction amount", Encrypt(amt));
            fields.Add("paymode", Encrypt(PayMode));
            return new GatewayForm
            {
                ActionUrl = PaymentUrl,
                Fields = fields
            };
        }

        /// <summary>
        /// Same as useraff.php:
        /// $getparam = '?merchantid=380738';
        /// foreach ($params as $keys => $values) {
        ///   if ($keys != "optional fields") $getparam .= '&'.$keys.'='.encrypt($values);
        ///   else $getparam .= '&'.$keys.'='.$values;
        /// }
        /// header('Location:'.$eazypay_Payment_url.$getparam);
        /// </summary>
        public static string BuildPaymentQuery(string refno, decimal amount, string studentName, string regno, string mobile, string email)
        {
            var amt = FormatAmount(amount);
            var mandatory = BuildPhpMandatory(refno, amt, studentName, regno, mobile, email);
            return "?merchantid=" + MerchantId
                + "&mandatory fields=" + Encrypt(mandatory)
                + "&optional fields="
                + "&returnurl=" + Encrypt(ReturnUrl)
                + "&Reference No=" + Encrypt(Clean(refno))
                + "&submerchantid=" + Encrypt(SubMerchantId)
                + "&transaction amount=" + Encrypt(amt)
                + "&paymode=" + Encrypt(PayMode);
        }

        public static string BuildPaymentRedirectUrl(string refno, decimal amount, string studentName, string regno, string mobile, string email)
        {
            return PaymentUrl + BuildPaymentQuery(refno, amount, studentName, regno, mobile, email);
        }

        /// <summary>
        /// useraff.php $getparam1 — same query as Location, values not encrypted.
        /// </summary>
        public static string BuildPaymentPlainUrl(string refno, decimal amount, string studentName, string regno, string mobile, string email)
        {
            var amt = FormatAmount(amount);
            var mandatory = BuildPhpMandatory(refno, amt, studentName, regno, mobile, email);
            return PaymentUrl
                + "?merchantid=" + MerchantId
                + "&mandatory fields=" + mandatory
                + "&optional fields="
                + "&returnurl=" + ReturnUrl
                + "&Reference No=" + Clean(refno)
                + "&submerchantid=" + SubMerchantId
                + "&transaction amount=" + amt
                + "&paymode=" + PayMode;
        }

        public static void StoreRefno(HttpSessionStateBase session, HttpResponseBase response, string refno)
        {
            if (session != null)
            {
                session[RefCookieName] = refno;
            }
            if (response == null || string.IsNullOrWhiteSpace(refno))
            {
                return;
            }

            // ICICI posts back from another site, so SameSite=Lax would drop Session.
            var cookie = new HttpCookie(RefCookieName, refno)
            {
                HttpOnly = true,
                Secure = true,
                Path = "/",
                Expires = DateTime.Now.AddHours(6)
            };
            cookie.SameSite = SameSiteMode.None;
            response.Cookies.Set(cookie);
        }

        public static void StorePayDebug(HttpResponseBase response, HttpSessionStateBase session, string mandatory, string plainUrl, string encUrl)
        {
            if (session != null)
            {
                session["AffPayPlain"] = mandatory;
                session["AffPayPlainUrl"] = plainUrl;
                session["AffPayUrl"] = encUrl;
            }
            if (response == null)
            {
                return;
            }
            SetCrossSiteCookie(response, "AffPayPlain", mandatory);
            SetCrossSiteCookie(response, "AffPayPlainUrl", plainUrl);
            SetCrossSiteCookie(response, "AffPayUrl", encUrl);
        }

        public static string ReadPayDebug(HttpSessionStateBase session, HttpRequestBase request, string sessionKey, string cookieName)
        {
            var value = session != null ? Convert.ToString(session[sessionKey]) : null;
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
            var cookie = request != null ? request.Cookies[cookieName] : null;
            if (cookie == null || string.IsNullOrEmpty(cookie.Value))
            {
                return string.Empty;
            }
            return Uri.UnescapeDataString(cookie.Value);
        }

        private static void SetCrossSiteCookie(HttpResponseBase response, string name, string value)
        {
            var cookie = new HttpCookie(name, Uri.EscapeDataString(value ?? string.Empty))
            {
                HttpOnly = true,
                Secure = true,
                Path = "/",
                Expires = DateTime.Now.AddHours(6)
            };
            cookie.SameSite = SameSiteMode.None;
            response.Cookies.Set(cookie);
        }

        public static string ReadStoredRefno(HttpSessionStateBase session, HttpRequestBase request)
        {
            var refno = session != null ? Convert.ToString(session[RefCookieName]) : null;
            if (IsRealRefno(refno))
            {
                return refno;
            }
            var cookie = request != null ? request.Cookies[RefCookieName] : null;
            return cookie != null && IsRealRefno(cookie.Value) ? cookie.Value : null;
        }

        public static string DescribeResponseCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return "No Response_Code was posted by ICICI.";
            }
            switch (code.Trim().ToUpperInvariant())
            {
                case "E000": return "Received successful confirmation.";
                case "E001": return "Unauthorized Payment Mode";
                case "E002": return "Unauthorized Key";
                case "E003": return "Unauthorized Packet";
                case "E004": return "Unauthorized Merchant";
                case "E005": return "Unauthorized Return URL";
                case "E006": return "Transaction is already paid";
                case "E007": return "Transaction Failed";
                case "E008": return "Failure from Third Party due to Technical Error";
                case "E009": return "Bill Already Expired";
                case "E0031": return "Mandatory fields coming from merchant are empty";
                case "E0032": return "Mandatory fields coming from database are empty";
                case "E0033": return "Payment mode coming from merchant is empty";
                case "E0034": return "PG Reference number coming from merchant is empty";
                case "E0035": return "Sub merchant id coming from merchant is empty";
                case "E0036": return "Transaction amount coming from merchant is empty";
                case "E0037": return "Payment mode coming from merchant is other than 0 to 9";
                case "E0038": return "Transaction amount coming from merchant is more than 9 digit length";
                case "E0039": return "Mandatory value Email in wrong format";
                case "E00310": return "Mandatory value mobile number in wrong format";
                case "E00311": return "Mandatory value amount in wrong format";
                case "E00321": return "Request packet mandatory columns is not equal to mandatory columns set in enrolment";
                case "E00322": return "Reference Number Blank";
                case "E00323": return "Mandatory Columns are Blank";
                case "E00324": return "Merchant Reference Number and Mandatory Columns are Blank";
                case "E00325": return "Merchant Reference Number Duplicate";
                case "E00326": return "Sub merchant id coming from merchant is non numeric";
                case "E00330": return "Transaction Amount and Mandatory Transaction Amount mismatch in Request URL";
                case "E00333": return "Referer is null/invalid Referer";
                case "E00334": return "Mandatory Parameters Reference No and Request Reference No parameter values are not matched";
                case "E00335": return "Transaction Cancelled By User";
                default: return "Bank response code from ICICI.";
            }
        }

        public static bool IsRealRefno(string refno)
        {
            return !string.IsNullOrWhiteSpace(refno)
                && !string.Equals(refno, "null", StringComparison.OrdinalIgnoreCase);
        }

        public static bool HasBankResponse(NameValueCollection form, NameValueCollection query)
        {
            return HasAny(form, "Response Code", "Response_Code", "mandatory fields", "mandatory_fields", "RS")
                || HasAny(query, "Response Code", "Response_Code", "mandatory fields", "mandatory_fields", "RS");
        }

        private static bool HasAny(NameValueCollection source, params string[] keys)
        {
            if (source == null || source.Count == 0)
            {
                return false;
            }
            foreach (var key in keys)
            {
                if (!string.IsNullOrEmpty(ReadField(source, key)))
                {
                    return true;
                }
            }
            return false;
        }

        public static GatewayResponse ParseResponse(NameValueCollection form)
        {
            var res = new GatewayResponse
            {
                MandatoryFields = First(form, "mandatory_fields", "mandatory fields"),
                MerchantId = First(form, "ID"),
                ResponseCode = First(form, "Response_Code", "Response Code"),
                UniqueRefNumber = First(form, "Unique_Ref_Number", "Unique Ref Number"),
                ServiceTaxAmount = First(form, "Service_Tax_Amount", "Service Tax Amount"),
                ProcessingFeeAmount = First(form, "Processing_Fee_Amount", "Processing Fee Amount"),
                TotalAmount = First(form, "Total_Amount", "Total Amount"),
                TransactionAmount = First(form, "Transaction_Amount", "Transaction Amount"),
                TransactionDate = First(form, "Transaction_Date", "Transaction Date"),
                InterchangeValue = First(form, "Interchange_Value", "Interchange Value"),
                Tdr = First(form, "TDR"),
                PaymentMode = First(form, "Payment_Mode", "Payment Mode"),
                SubMerchantId = First(form, "SubMerchantId"),
                ReferenceNo = First(form, "ReferenceNo", "Reference No"),
                Tps = First(form, "TPS"),
                Rs = First(form, "RS")
            };

            if (!string.IsNullOrEmpty(res.MandatoryFields))
            {
                var parts = res.MandatoryFields.Split('|');
                if (parts.Length > 0 && string.IsNullOrEmpty(res.ReferenceNo)) res.ReferenceNo = parts[0];
                if (parts.Length > 3) res.StudentName = parts[3];
                if (parts.Length > 4) res.Regno = parts[4];
                if (parts.Length > 5) res.Uid = parts[5];
                if (parts.Length > 7) res.Mobile = parts[7];
                if (parts.Length > 8) res.Email = parts[8];
            }

            var hashData = string.Join("|", new[]
            {
                res.MerchantId,
                res.ResponseCode,
                res.UniqueRefNumber,
                res.ServiceTaxAmount,
                res.ProcessingFeeAmount,
                res.TotalAmount,
                res.TransactionAmount,
                res.TransactionDate,
                res.InterchangeValue,
                res.Tdr,
                res.PaymentMode,
                res.SubMerchantId,
                res.ReferenceNo,
                res.Tps,
                AesKey
            });

            res.HashValid = !string.IsNullOrEmpty(res.Rs)
                && string.Equals(Sha512(hashData), res.Rs, StringComparison.OrdinalIgnoreCase);

            res.IsSuccess = string.Equals((res.ResponseCode ?? string.Empty).Trim(), "E000", StringComparison.OrdinalIgnoreCase)
                && res.HashValid;
            res.BankStatus = res.IsSuccess ? "Success" : null;
            return res;
        }

        public static bool IsPaidStatus(string bankStatus)
        {
            return string.Equals((bankStatus ?? string.Empty).Trim(), "Success", StringComparison.OrdinalIgnoreCase);
        }

        public static PaymentConfirm Confirm(GatewayResponse browser, AffAppPaymentRow existing)
        {
            var confirm = new PaymentConfirm();
            if (existing != null && IsPaidStatus(existing.bankstatus))
            {
                confirm.BankStatus = "Success";
                confirm.Amount = existing.transactionamount ?? existing.pgamount;
                confirm.UniqueRef = existing.uniquerefnumber;
                confirm.TransactionDate = existing.transactiondate;
                confirm.ApplyUpdate = false;
                return confirm;
            }

            var refno = browser != null && IsRealRefno(browser.ReferenceNo)
                ? browser.ReferenceNo
                : (existing != null ? existing.refno : null);
            var verify = IsRealRefno(refno) ? VerifyByRefno(refno) : new VerifyResult();
            var code = browser != null ? (browser.ResponseCode ?? string.Empty).Trim().ToUpperInvariant() : string.Empty;

            if (IsVerifySuccess(verify.Status))
            {
                confirm.BankStatus = "Success";
                confirm.UniqueRef = FirstNonEmpty(verify.EzpayTranId, browser != null ? browser.UniqueRefNumber : null);
                confirm.TransactionDate = FirstNonEmpty(verify.TranDate, browser != null ? browser.TransactionDate : null);
                confirm.Amount = ParseAmount(verify.Amount)
                    ?? (browser != null ? ParseAmount(browser.TransactionAmount) : null)
                    ?? (existing != null ? existing.pgamount : null);
                confirm.ApplyUpdate = true;
                return confirm;
            }

            var merchantOk = browser == null
                || string.IsNullOrWhiteSpace(browser.MerchantId)
                || string.Equals(browser.MerchantId, MerchantId, StringComparison.OrdinalIgnoreCase);
            if (code == "E000" && browser != null && browser.HashValid && merchantOk)
            {
                confirm.BankStatus = "Success";
                confirm.UniqueRef = browser.UniqueRefNumber;
                confirm.TransactionDate = browser.TransactionDate;
                confirm.Amount = ParseAmount(browser.TransactionAmount)
                    ?? (existing != null ? existing.pgamount : null);
                confirm.ApplyUpdate = true;
                return confirm;
            }

            if (code == "E006")
            {
                confirm.BankStatus = "Success";
                confirm.UniqueRef = FirstNonEmpty(verify.EzpayTranId, browser != null ? browser.UniqueRefNumber : null);
                confirm.TransactionDate = FirstNonEmpty(verify.TranDate, browser != null ? browser.TransactionDate : null);
                confirm.Amount = ParseAmount(verify.Amount)
                    ?? (browser != null ? ParseAmount(browser.TransactionAmount) : null)
                    ?? (existing != null ? existing.pgamount : null);
                confirm.ApplyUpdate = true;
                return confirm;
            }

            if (IsDeclineCode(code) || IsVerifyFail(verify.Status))
            {
                confirm.BankStatus = "Declined";
                confirm.UniqueRef = browser != null ? NullIfEmpty(browser.UniqueRefNumber) : null;
                confirm.TransactionDate = browser != null ? NullIfEmpty(browser.TransactionDate) : null;
                confirm.Amount = browser != null ? ParseAmount(browser.TransactionAmount) : null;
                confirm.ApplyUpdate = true;
                return confirm;
            }

            confirm.ApplyUpdate = false;
            return confirm;
        }

        public static VerifyResult VerifyByRefno(string refno)
        {
            var result = new VerifyResult();
            if (!IsRealRefno(refno))
            {
                return result;
            }
            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                var url = VerifyUrl
                    + "?ezpaytranid=&amount=&paymentmode="
                    + "&merchantid=" + Uri.EscapeDataString(MerchantId)
                    + "&trandate="
                    + "&pgreferenceno=" + Uri.EscapeDataString(refno)
                    + "&dstatus=Y";
                var req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";
                req.Timeout = 15000;
                req.ReadWriteTimeout = 15000;
                req.UserAgent = "AKV-ExamEnq-EazyPGVerify/1.0";
                using (var resp = (HttpWebResponse)req.GetResponse())
                using (var stream = resp.GetResponseStream())
                using (var reader = new StreamReader(stream ?? Stream.Null, Encoding.UTF8))
                {
                    result.Raw = (reader.ReadToEnd() ?? string.Empty).Trim();
                }
                var parsed = HttpUtility.ParseQueryString(result.Raw);
                result.Status = parsed["status"] ?? string.Empty;
                result.EzpayTranId = parsed["ezpaytranid"];
                result.Amount = FirstNonEmpty(parsed["BA"], parsed["ba"], parsed["amount"]);
                result.TranDate = parsed["trandate"];
                if (string.IsNullOrEmpty(result.Status) && result.Raw.IndexOf('=') < 0)
                {
                    result.Status = result.Raw;
                }
            }
            catch (Exception ex)
            {
                result.Status = "verify-error";
                result.Raw = ex.Message;
            }
            return result;
        }

        private static bool IsVerifySuccess(string status)
        {
            var s = (status ?? string.Empty).Trim().ToLowerInvariant();
            return s == "success" || s == "rip" || s == "sip" || s.IndexOf("success", StringComparison.Ordinal) >= 0;
        }

        private static bool IsVerifyFail(string status)
        {
            var s = (status ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(s) || s == "verify-error" || s == "notinitiated")
            {
                return false;
            }
            return s == "failed" || s == "timeout" || s == "transaction expired" || s == "user dropped"
                || s == "cheque/dd returned" || s == "canceled by user" || s == "cancelled by user"
                || s.IndexOf("fail", StringComparison.Ordinal) >= 0
                || s.IndexOf("timeout", StringComparison.Ordinal) >= 0
                || s.IndexOf("expir", StringComparison.Ordinal) >= 0;
        }

        private static bool IsDeclineCode(string code)
        {
            return code == "E00335" || code == "E007" || code == "E009";
        }

        private static decimal? ParseAmount(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw) || string.Equals(raw, "NA", StringComparison.OrdinalIgnoreCase)
                || string.Equals(raw, "null", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            decimal value;
            if (decimal.TryParse(raw.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            {
                return value;
            }
            return null;
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null)
            {
                return null;
            }
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value)
                    && !string.Equals(value, "NA", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(value, "null", StringComparison.OrdinalIgnoreCase))
                {
                    return value;
                }
            }
            return null;
        }

        private static string NullIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        public static string Encrypt(string textToEncrypt)
        {
            // useraff.php eazypay_aes128Encrypt: PKCS7 pad, then rijndael_128 ECB (mcrypt uses zero IV)
            var plain = Encoding.UTF8.GetBytes(textToEncrypt ?? string.Empty);
            var block = 16;
            var pad = block - (plain.Length % block);
            if (pad == 0)
            {
                pad = block;
            }
            var padded = new byte[plain.Length + pad];
            Buffer.BlockCopy(plain, 0, padded, 0, plain.Length);
            for (var i = plain.Length; i < padded.Length; i++)
            {
                padded[i] = (byte)pad;
            }

            using (var rijndaelCipher = new RijndaelManaged())
            {
                rijndaelCipher.Mode = CipherMode.ECB;
                rijndaelCipher.Padding = PaddingMode.None;
                rijndaelCipher.KeySize = 0x80;
                rijndaelCipher.BlockSize = 0x80;
                var keyBytes = new byte[0x10];
                var pwdBytes = Encoding.UTF8.GetBytes(AesKey);
                Array.Copy(pwdBytes, keyBytes, Math.Min(pwdBytes.Length, keyBytes.Length));
                rijndaelCipher.Key = keyBytes;
                rijndaelCipher.IV = new byte[0x10];
                using (var transform = rijndaelCipher.CreateEncryptor())
                {
                    return Convert.ToBase64String(transform.TransformFinalBlock(padded, 0, padded.Length));
                }
            }
        }

        private static string Clean(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            return value.Replace("\r", string.Empty).Replace("\n", string.Empty);
        }

        public static string Sha512(string input)
        {
            using (var sha = SHA512.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input ?? string.Empty));
                var sb = new StringBuilder(hash.Length * 2);
                for (var i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        private static string FormatAmount(decimal amount)
        {
            return decimal.Truncate(amount).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        private static string First(NameValueCollection form, params string[] keys)
        {
            if (form == null) return string.Empty;
            foreach (var key in keys)
            {
                var value = ReadField(form, key);
                if (!string.IsNullOrEmpty(value)) return value;
            }
            return string.Empty;
        }

        private static string ReadField(NameValueCollection form, string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            foreach (var candidate in new[] { key, key.Replace('_', ' '), key.Replace(' ', '_'), key.Replace(' ', '+') })
            {
                var value = form[candidate];
                if (!string.IsNullOrEmpty(value) && !string.Equals(value, "null", StringComparison.OrdinalIgnoreCase))
                {
                    return value;
                }
            }
            return string.Empty;
        }

        private static string Setting(string key, string fallback)
        {
            var value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }
    }
}
