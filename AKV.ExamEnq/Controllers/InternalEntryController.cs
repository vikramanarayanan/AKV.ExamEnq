using AKV.ExamEnq.Models;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;

namespace AKV.ExamEnq.Controllers
{
    [Authorize] 
    public class InternalEntryController : Controller
    {
        // GET: InternalEntry
        [Authorize(Users = "tryvikraman@gmail.com")]
       //  [Authorize]
        public ActionResult Index()
        {
            return View("InternalEntry");
        }
        [Authorize(Users = "tryvikraman@gmail.com")]
        public ActionResult IndexRegu()
        {
            return View("InternalEntryRegu");
        }

        public JsonResult GetInternalPapers()
        {
          
           var course = Request.Params["rid"];
           
            var namefilter =  Request.Params["namefilter"];
           
            return Json(MarkRepository.Repository.GetInternalPapersByCourse(course,namefilter), JsonRequestBehavior.AllowGet);
        }
      //internal tag lock method
      public ActionResult IntTag(string coursecode,string pap_no)
        {if (!string.IsNullOrEmpty(coursecode) && !string.IsNullOrEmpty(pap_no))
                {
                MarkRepository.Repository.Inttag(coursecode, pap_no);
            }
            return View("InternalEntry");
        }
        
        // grid action method
        public ActionResult ReadMark([DataSourceRequest] DataSourceRequest request,string course, string pap_no)
        {
            return Json(MarkRepository.Repository.GetMark_Internal(course,pap_no).ToDataSourceResult(request));
        }

        public ActionResult UpdateInternalMark([DataSourceRequest] DataSourceRequest request, MarkViewModel mark)
        {
            if (ModelState.IsValid)
            {

                MarkRepository.Repository.UpdateSesMark(mark);

            }


            return Json(new[] { mark }.ToDataSourceResult(request, ModelState));
        }

    }
}