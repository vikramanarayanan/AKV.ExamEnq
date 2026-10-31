using AKV.ExamEnq.Models;
using AKV.ExamEnq.Report;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace AKV.ExamEnq.Controllers
{
    [AllowAnonymous]
    public class AffAppController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            if (!IsBankAffAppPath())
            {
                return RedirectToRoute("BankAffApp");
            }
            return View(AffAppRepository.GetYearSettings());
        }

        [HttpGet]
        public ActionResult AppStatus()
        {
            return View(AffAppRepository.GetAppStatus());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public ActionResult IndexPost(AffAppPayRequestVm model)
        {
            return Pay(model);
        }

        [HttpGet]
        public JsonResult Search(string regno)
        {
            regno = (regno ?? string.Empty).Trim();
            var result = new AffAppSearchResultVm
            {
                papers = new List<AffAppPaperVm>(),
                receipts = new List<AffAppReceiptLinkVm>()
            };
            if (regno.Length > 17 || regno.IndexOfAny(new[] { '\'', ';' }) >= 0)
            {
                result.error = "Invalid register number.";
                return JsonResult(result);
            }

            try
            {
                if (string.IsNullOrEmpty(regno))
                {
                    result.error = "Please Enter Register Number!";
                    return JsonResult(result);
                }

                try
                {
                    var paid = AffAppRepository.GetSuccessPayments(regno);
                    if (paid != null && paid.Count > 0)
                    {
                        result.alreadyPaid = true;
                        result.receipts = paid.Select(x => new AffAppReceiptLinkVm
                        {
                            refno = x.refno
                        }).ToList();
                        return JsonResult(result);
                    }
                }
                catch
                {
                    // Payment log lookup should not block student search
                }

                var student = AffAppRepository.GetStudent(regno);
                if (student == null)
                {
                    result.error = "Register Number Not Found!";
                    return JsonResult(result);
                }

                var papers = AffAppRepository.GetPapers(regno);
                result.coursecode = student.coursecode;
                result.coursename = student.coursename;
                result.collegecode = student.collegecode;
                result.collegename = student.collegename;
                result.name = student.name;
                result.regno = student.regno ?? student.rollno;
                result.rollno = student.rollno;
                result.sem_no = student.sem_no;
                var year = AffAppRepository.GetYearSettings();
                result.demand = student.demand;
                result.paymentAllowed = year.showPayment;
                result.fine = year.fine;
                result.photo1 = student.photo != null ? Convert.ToBase64String(student.photo) : null;
                result.papers = papers.Select(p => new AffAppPaperVm
                {
                    sem_no = p.sem_no,
                    pap_no = p.pap_no,
                    papname = p.papname,
                    papfee = p.papfee
                }).ToList();

                return JsonResult(result);
            }
            catch (Exception ex)
            {
                result.error = ex.Message;
                return JsonResult(result);
            }
        }

        private JsonResult JsonResult(object data)
        {
            return new JsonResult
            {
                Data = data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = int.MaxValue
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Pay(AffAppPayRequestVm model)
        {
            var regno = ((model != null ? model.Regno : null) ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(regno))
            {
                TempData["AffError"] = "Please Enter Register Number!";
                return RedirectToRoute("BankAffApp");
            }
            if (regno.Length > 17)
            {
                TempData["AffError"] = "Register Number maximum 17 characters.";
                return RedirectToRoute("BankAffApp");
            }

            try
            {
                try
                {
                    var existing = AffAppRepository.GetSuccessPayments(regno);
                    if (existing != null && existing.Count > 0)
                    {
                        EazypayService.StoreRefno(Session, Response, existing[0].refno);
                        return DownloadReceiptPdf();
                    }
                }
                catch
                {
                }

                var year = AffAppRepository.GetYearSettings();
                if (!year.showPayment)
                {
                    TempData["AffError"] = "Examination Fee Online Payment Portal Closed";
                    return RedirectToRoute("BankAffApp");
                }

                var student = AffAppRepository.GetStudent(regno);
                var papers = AffAppRepository.GetPapers(regno);
                if (student == null || string.IsNullOrWhiteSpace(student.name) || student.demand <= 0)
                {
                    TempData["AffError"] = "Please Select Paper or Name or Faculty";
                    return RedirectToRoute("BankAffApp");
                }

                var paperCodes = string.Join("|", papers.Select(p => p.pap_no).Where(p => !string.IsNullOrWhiteSpace(p)));
                if (!string.IsNullOrEmpty(paperCodes)) paperCodes += "|";

                var refno = AffAppRepository.NextBankUid();
                var email = "test@gmail.com";
                var mobile = "9999999999";

                AffAppRepository.InsertPayment(new AffAppPaymentInsert
                {
                    refno = refno,
                    submerchantid = EazypayService.SubMerchantId,
                    pgamount = student.demand,
                    stname = student.name,
                    email = email,
                    mobileno = mobile,
                    regno = student.regno ?? student.rollno,
                    rollno = student.rollno ?? string.Empty,
                    uid = EazypayService.Uid,
                    fcode = EazypayService.Fcode,
                    semester = student.sem_no,
                    coursecode = student.coursecode,
                    mode = EazypayService.Uid,
                    papers = paperCodes,
                    faculty = string.Empty,
                    token = string.Empty,
                    provfees = 0,
                    degfees = 0,
                    confees = 0,
                    coursename = student.coursename,
                    collegecode = student.collegecode,
                    collegename = student.collegename,
                    fine = year.fine,
                    id = student.id
                });

                EazypayService.StoreRefno(Session, Response, refno);
                return Redirect("~/bank/startpay");
            }
            catch (Exception ex)
            {
                TempData["AffError"] = ex.Message;
                return RedirectToRoute("BankAffApp");
            }
        }

        [HttpGet]
        public ActionResult ToBank()
        {
            return Redirect("~/bank/startpay");
        }

        private bool IsBankAffAppPath()
        {
            var path = Request.Path ?? string.Empty;
            return path.IndexOf("/bank/AffApp", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult OpenReceipt(string refno)
        {
            if (!EazypayService.IsRealRefno(refno))
            {
                return new HttpStatusCodeResult(400);
            }
            EazypayService.StoreRefno(Session, Response, refno);
            return DownloadReceiptPdf(true);
        }

        [HttpGet]
        public ActionResult Receipt()
        {
            if (!EazypayService.CanShowReceiptPage(Session))
            {
                return DownloadReceiptPdf();
            }

            var refno = EazypayService.ReadStoredRefno(Session, Request);
            if (!EazypayService.IsRealRefno(refno))
            {
                return RedirectToRoute("BankAffApp");
            }
            var row = AffAppRepository.GetByRefno(refno);
            var isSuccess = row != null && EazypayService.IsPaidStatus(row.bankstatus);
            var isPending = row != null && string.IsNullOrWhiteSpace(row.bankstatus);
            var message = isPending
                ? "Payment is being confirmed. Please wait a few minutes or search the register number again."
                : null;
            if (row == null)
            {
                return View(new AffAppReceiptVm
                {
                    refno = refno,
                    bankstatus = "Declined",
                    isSuccess = false,
                    isPending = false,
                    message = "Payment reference not found."
                });
            }

            return View(new AffAppReceiptVm
            {
                refno = row.refno,
                stname = row.stname,
                regno = row.regno,
                uniquerefnumber = row.uniquerefnumber,
                transactiondate = row.transactiondate,
                transactionamount = Convert.ToString(row.transactionamount ?? row.pgamount ?? 0),
                bankstatus = isSuccess ? "Success" : (isPending ? null : row.bankstatus),
                isSuccess = isSuccess,
                isPending = isPending,
                message = message
            });
        }

        [HttpGet]
        public ActionResult Report()
        {
            return DownloadReceiptPdf();
        }

        private ActionResult DownloadReceiptPdf(bool fromLink = false)
        {
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            var refno = EazypayService.ReadStoredRefno(Session, Request);
            if (!EazypayService.IsRealRefno(refno))
            {
                return fromLink
                    ? (ActionResult)new HttpStatusCodeResult(400)
                    : RedirectToRoute("BankAffApp");
            }

            var row = AffAppRepository.GetSuccessByRefno(refno);
            if (row == null)
            {
                return fromLink
                    ? (ActionResult)new HttpStatusCodeResult(404)
                    : Content("No data found");
            }

            var data = new List<AffAppReportPoco> { AffAppRepository.ToReport(row) };
            var rpt = new AffAppReport();
            rpt.Load();
            rpt.SetDataSource(data);
            Stream s = rpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            rpt.Close();
            rpt.Dispose();
            return File(s, "application/pdf", "AffExamFee.pdf");
        }
    }
}
