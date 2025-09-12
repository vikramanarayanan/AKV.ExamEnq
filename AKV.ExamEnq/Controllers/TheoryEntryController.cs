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
    [Authorize(Users = "tryvikraman@gmail.com")]
   // [Authorize]
    public class TheoryEntryController : Controller
    {
        // GET: TheoryEntry
        public ActionResult Index()
        {
            return View("TheoryEntry");
        }


        public JsonResult GetTheoryPapers()
        {

            var course = Request.Params["rid"];

            var namefilter = Request.Params["namefilter"];

            return Json(MarkRepository.Repository.GetTheoryPapersByCourse(course, namefilter), JsonRequestBehavior.AllowGet);
        }


        // grid action method
        public ActionResult ReadMark([DataSourceRequest] DataSourceRequest request, string course, string pap_no)
        {//Getmark but Getmark_inernal
            return Json(MarkRepository.Repository.GetMark_Internal(course, pap_no).ToDataSourceResult(request));
        }

        public ActionResult UpdateTheoryMark([DataSourceRequest] DataSourceRequest request, MarkViewModel mark)
        {
            if (ModelState.IsValid)
            {

                MarkRepository.Repository.UpdateExtMark(mark);

            }


            return Json(new[] { mark }.ToDataSourceResult(request, ModelState));
        }
    }
}