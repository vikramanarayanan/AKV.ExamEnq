using AKV.ExamEnq.Models;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AKV.ExamEnq.Controllers
{
   // [Authorize]
[Authorize(Users = "tryvikraman@gmail.com")]
    public class AbsentEntryController : Controller
    {
        // GET: AbsentEntry
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetAbsentPapers()
        {

            var course = Request.Params["rid"];

            var namefilter = Request.Params["namefilter"];

            return Json(AbsentRepository.Repository.GetAbsentPapersByCourse(course, namefilter), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAbsent()
        {

            var course = Request.Params["rid"];

            var namefilter = Request.Params["namefilter"];

            return Json(AbsentRepository.Repository.GetAbsent(course, namefilter), JsonRequestBehavior.AllowGet);
        }


        // grid action method
        public ActionResult ReadAbsent([DataSourceRequest] DataSourceRequest request, string course, string pap_no)
        {
            return Json(AbsentRepository.Repository.GetAbsent(course, pap_no).ToDataSourceResult(request));
        }

        public ActionResult UpdateAbsent([DataSourceRequest] DataSourceRequest request,AbsentViewModel absent)
        {
            if (ModelState.IsValid)
            {

                AbsentRepository.Repository.UpdateAbsent(absent);

            }


            return Json(new[] { absent }.ToDataSourceResult(request, ModelState));
        }

    }
}