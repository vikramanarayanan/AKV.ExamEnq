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
    [Authorize]
  //[Authorize(Users = "tryvikraman@gmail.com")]
    public class PracticalEntryController : Controller
    {
        // GET: PracticalEntry
      [Authorize(Users = "tryvikraman@gmail.com")]
       // [Authorize]
        public ActionResult Index()
        {
            return View("PracticalEntry");
        }
        public ActionResult IndexRegu()
        {
            return View("PracticalEntryRegu");
        }


        public JsonResult GetPracticalPapers()
        {

            var course = Request.Params["rid"];

            var namefilter = Request.Params["namefilter"];

            return Json(MarkRepository.Repository.GetPracticalPapersByCourse(course, namefilter), JsonRequestBehavior.AllowGet);
        }
        //Practical tag lock method
        public ActionResult PracTag(string coursecode, string pap_no)
        {
            if (!string.IsNullOrEmpty(coursecode) && !string.IsNullOrEmpty(pap_no))
            {
                MarkRepository.Repository.Practag(coursecode, pap_no);
            }
            return View("PracticalEntry");
        }
        // grid action method
        public ActionResult ReadMark([DataSourceRequest] DataSourceRequest request, string course, string pap_no)
        {
            return Json(MarkRepository.Repository.GetMark_Practical(course, pap_no).ToDataSourceResult(request));
        }

        public ActionResult UpdatePracticalMark([DataSourceRequest] DataSourceRequest request, MarkViewModel mark)
        {
            if (ModelState.IsValid)
            {

                MarkRepository.Repository.UpdateExtMark(mark);

            }


            return Json(new[] { mark }.ToDataSourceResult(request, ModelState));
        }


    }
}