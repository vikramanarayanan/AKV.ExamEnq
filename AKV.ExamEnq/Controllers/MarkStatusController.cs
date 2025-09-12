using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static AKV.ExamEnq.Controllers.MarkStatusController;

namespace AKV.ExamEnq.Controllers
{
    public class MarkStatusController : Controller
    {
        public class MarkstatusPoco
        {
            public MarkstatusPoco()
            {
                marklist = new List<MarkstatusPoco>();
               
            }
            public List<MarkstatusPoco> marklist { get; set; }
            public string collegecode { get; set; }
            public string coursecode { get; set; }
            public decimal total { get; set; }
            public decimal ext_entered { get; set; }
            public decimal ext_null { get; set; }
            public decimal ses_entered { get; set; }
            public decimal ses_null { get; set; }
           
        }


        // GET: MarkStatus
        public ActionResult MarkStatus()
        {
            MarkstatusPoco list = new MarkstatusPoco();
            return View(list);
        }
    }
}