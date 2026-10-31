using AKV.ExamEnq.Models;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web.Mvc;

namespace AKV.ExamEnq.Controllers
{
    [AllowAnonymous]
    public class BankController : Controller
    {
        [HttpGet]
        public ActionResult AffApp()
        {
            return RedirectToRoute("BankAffApp");
        }

        [HttpGet]
        public ActionResult StartPay()
        {
            NoStore();
            return LaunchPendingPayment();
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult ReturnUrl()
        {
            NoStore();
            if (!EazypayService.HasBankResponse(Request.Unvalidated.Form, Request.Unvalidated.QueryString))
            {
                return GoToReceiptOrHome();
            }

            var form = new NameValueCollection();
            var dump = new AffAppBankReturnVm
            {
                fields = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, string>>(),
                httpMethod = Request.HttpMethod,
                requestUrl = Convert.ToString(Request.Url),
                referrer = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : string.Empty,
                rawQuery = Request.Url != null ? Request.Url.Query : string.Empty,
                rawBody = ReadRawBody(),
                sentMandatory = EazypayService.ReadPayDebug(Session, Request, "AffPayPlain", "AffPayPlain"),
                sentPlainUrl = EazypayService.ReadPayDebug(Session, Request, "AffPayPlainUrl", "AffPayPlainUrl"),
                sentEncUrl = EazypayService.ReadPayDebug(Session, Request, "AffPayUrl", "AffPayUrl")
            };

            try
            {
                AddAll(form, Request.Unvalidated.Form);
                AddAll(form, Request.Unvalidated.QueryString);
                TryLogReturn(form);

                if (form.AllKeys != null)
                {
                    foreach (string key in form.AllKeys)
                    {
                        dump.fields.Add(new System.Collections.Generic.KeyValuePair<string, string>(key ?? "(null)", form[key]));
                    }
                }

                var response = EazypayService.ParseResponse(form);
                dump.responseCode = response.ResponseCode;
                dump.responseMeaning = EazypayService.DescribeResponseCode(response.ResponseCode);
                dump.refno = response.ReferenceNo;
                if (!EazypayService.IsRealRefno(dump.refno))
                {
                    dump.refno = EazypayService.ReadStoredRefno(Session, Request);
                    if (EazypayService.IsRealRefno(dump.refno))
                    {
                        response.ReferenceNo = dump.refno;
                    }
                }

                if (EazypayService.IsRealRefno(dump.refno)
                    && (string.IsNullOrEmpty(response.Uid)
                        || string.Equals(response.Uid, EazypayService.Uid, StringComparison.OrdinalIgnoreCase)))
                {
                    var existing = AffAppRepository.GetByRefno(dump.refno);
                    var confirm = EazypayService.Confirm(response, existing);
                    dump.bankStatus = confirm.BankStatus;
                    if (confirm.ApplyUpdate && !string.IsNullOrEmpty(confirm.BankStatus))
                    {
                        AffAppRepository.UpdateBankStatus(
                            dump.refno,
                            confirm.Amount,
                            confirm.UniqueRef,
                            confirm.TransactionDate,
                            confirm.BankStatus);
                    }
                }

                if (EazypayService.IsRealRefno(dump.refno))
                {
                    return RedirectToReceipt(dump.refno);
                }
            }
            catch (Exception ex)
            {
                dump.exception = ex.ToString();
                if (string.IsNullOrWhiteSpace(dump.responseMeaning))
                {
                    dump.responseMeaning = EazypayService.DescribeResponseCode(dump.responseCode);
                }
            }

            if (EazypayService.IsRealRefno(dump.refno))
            {
                return RedirectToReceipt(dump.refno);
            }
            return RedirectToRoute("BankAffApp");
        }

        private string ReadRawBody()
        {
            try
            {
                if (Request.InputStream == null || !Request.InputStream.CanRead)
                {
                    return string.Empty;
                }
                if (Request.InputStream.CanSeek)
                {
                    Request.InputStream.Position = 0;
                }
                using (var reader = new StreamReader(Request.InputStream, Encoding.UTF8, true, 1024, true))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private static void AddAll(NameValueCollection target, NameValueCollection source)
        {
            if (source == null)
            {
                return;
            }
            foreach (string key in source.AllKeys)
            {
                target.Add(key, source[key]);
            }
        }

        private void FillSentPacket(AffAppBankReturnVm dump)
        {
            if (!string.IsNullOrWhiteSpace(dump.sentPlainUrl) && !string.IsNullOrWhiteSpace(dump.sentEncUrl))
            {
                return;
            }
            var refno = dump.refno;
            if (!EazypayService.IsRealRefno(refno))
            {
                refno = EazypayService.ReadStoredRefno(Session, Request);
            }
            if (!EazypayService.IsRealRefno(refno))
            {
                return;
            }
            var row = AffAppRepository.GetByRefno(refno);
            if (row == null)
            {
                return;
            }
            var amount = row.pgamount ?? 0;
            var mobile = "9999999999";
            var email = "test@gmail.com";
            if (string.IsNullOrWhiteSpace(dump.sentMandatory))
            {
                dump.sentMandatory = EazypayService.BuildPhpMandatory(
                    row.refno,
                    Convert.ToString(decimal.Truncate(amount)),
                    row.stname,
                    row.regno,
                    mobile,
                    email);
            }
            if (string.IsNullOrWhiteSpace(dump.sentPlainUrl))
            {
                dump.sentPlainUrl = EazypayService.BuildPaymentPlainUrl(row.refno, amount, row.stname, row.regno, mobile, email);
            }
            if (string.IsNullOrWhiteSpace(dump.sentEncUrl))
            {
                dump.sentEncUrl = EazypayService.BuildPaymentRedirectUrl(row.refno, amount, row.stname, row.regno, mobile, email);
            }
        }

        private void TryLogReturn(NameValueCollection form)
        {
            try
            {
                var dir = Server.MapPath("~/App_Data");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                var sb = new StringBuilder();
                sb.Append(DateTime.Now.ToString("s")).Append(' ');
                if (form != null)
                {
                    foreach (string key in form.AllKeys)
                    {
                        sb.Append(key).Append('=').Append(form[key]).Append("; ");
                    }
                }
                sb.AppendLine();
                System.IO.File.AppendAllText(Path.Combine(dir, "eazypay-return.log"), sb.ToString());
            }
            catch
            {
            }
        }

        private void NoStore()
        {
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
        }

        private ActionResult RedirectToReceipt(string refno)
        {
            if (EazypayService.IsRealRefno(refno))
            {
                EazypayService.StoreRefno(Session, Response, refno);
            }
            EazypayService.AllowReceiptPage(Session);
            return RedirectToRoute("BankAffAppActions", new { action = "Receipt" });
        }

        private ActionResult GoToReceiptOrHome()
        {
            var refno = EazypayService.ReadStoredRefno(Session, Request);
            if (!EazypayService.IsRealRefno(refno))
            {
                return RedirectToRoute("BankAffApp");
            }
            return RedirectToReceipt(refno);
        }

        private ActionResult LaunchPendingPayment()
        {
            var refno = EazypayService.ReadStoredRefno(Session, Request);
            if (!EazypayService.IsRealRefno(refno))
            {
                return RedirectToRoute("BankAffApp");
            }

            var row = AffAppRepository.GetByRefno(refno);
            if (row == null)
            {
                return RedirectToRoute("BankAffApp");
            }
            if (EazypayService.IsPaidStatus(row.bankstatus) || !string.IsNullOrWhiteSpace(row.bankstatus))
            {
                EazypayService.StoreRefno(Session, Response, row.refno);
                return RedirectToRoute("BankAffAppActions", new { action = "Report" });
            }

            var amount = row.pgamount ?? 0;
            var mobile = "9999999999";
            var email = "test@gmail.com";
            var bankUrl = EazypayService.BuildPaymentRedirectUrl(row.refno, amount, row.stname, row.regno, mobile, email);
            var plain = EazypayService.BuildPhpMandatory(
                row.refno,
                Convert.ToString(decimal.Truncate(amount)),
                row.stname,
                row.regno,
                mobile,
                email);
            var plainUrl = EazypayService.BuildPaymentPlainUrl(row.refno, amount, row.stname, row.regno, mobile, email);
            EazypayService.StorePayDebug(Response, Session, plain, plainUrl, bankUrl);
            Response.Headers["Referrer-Policy"] = "unsafe-url";
            return View("~/Views/AffApp/RedirectToBank.cshtml", new AffAppRedirectVm
            {
                PlainUrl = plainUrl,
                EncryptedUrl = bankUrl,
                BankUrl = EazypayService.ReturnUrl,
                MandatoryPlain = plain,
                AutoSubmit = true,
                Form = EazypayService.BuildPaymentForm(row.refno, amount, row.stname, row.regno, mobile, email)
            });
        }
    }
}
