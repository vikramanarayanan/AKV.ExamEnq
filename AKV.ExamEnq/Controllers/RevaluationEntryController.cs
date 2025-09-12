using AKV.ExamEnq.Models;
using AKV.ExamEnq.Report;
using AKV.ExanEnq.Entity;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AKV.ExamEnq.Controllers
{
    public class RevaluationEntryController : Controller
    {
        // GET: RevaluationEntry
        //  [Authorize]
        [Authorize(Users = "tryvikraman@gmail.com")]
        public ActionResult RevaluationEntry()
        {
            return View();
        }

        public JsonResult GetRevaluationPapers()
        {

            var course = Request.Params["rid"];

            var namefilter = Request.Params["namefilter"];

            return Json(RevaluationRepository.Repository.GetRevaluiationPapersByCourse(course, namefilter), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetRevaluation()
        {

            var course = Request.Params["rid"];

            var namefilter = Request.Params["namefilter"];

            return Json(RevaluationRepository.Repository.GetRevaluation(course, namefilter), JsonRequestBehavior.AllowGet);
        }


        // grid action method
        public ActionResult ReadRevaluation([DataSourceRequest] DataSourceRequest request, string course, string pap_no)
        {
            return Json(RevaluationRepository.Repository.GetRevaluation(course, pap_no).ToDataSourceResult(request));
        }
        public ActionResult RevaluationReportDownload(string coursecode,string pap_no)
        {



            var ccode = CollegeRepository.GetLoginCollege();

            using (Entities et = new Entities())
            {
                var data = et.Database.SqlQuery<AbsentPoco>("select * from view_revaluation where status = 'Reval' and collegecode = '" + ccode + "' and coursecode = '" + coursecode + "' and pap_no = '" + pap_no + "' order by regno").ToList();

                RevalReport rpt = new RevalReport();
                rpt.Load();
                rpt.SetDataSource(data);
                Stream s = rpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                rpt.Close();
                rpt.Dispose();
                return File(s, "application/pdf");

            }
        }
        public ActionResult UpdateRevaluation([DataSourceRequest] DataSourceRequest request, AbsentViewModel absent)
        {
            if (ModelState.IsValid)
            {

                RevaluationRepository.Repository.UpdateRevaluation(absent);

            }


            return Json(new[] { absent }.ToDataSourceResult(request, ModelState));
        }



    }
}