using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AKV.ExamEnq.Models;
using AKV.ExamEnq.Report;
using AKV.ExanEnq.Entity;

namespace AKV.ExamEnq.Controllers
{
    [Authorize(Users = "tryvikraman@gmail.com")]
    public class CourseWiseStrReportController : Controller
    {
      

        public ActionResult CourseWiseStrCheckList(string coursecode)
        {


            var ccode = CollegeRepository.GetLoginCollege();

            using (Entities et = new Entities())
            {
                 var data = et.Database.SqlQuery<CourseWisePoco>("select * from coursewise_str_new where collegecode = '" + ccode + "'  order by coursecode ").ToList();

                

                CourseWiseStrReport rpt = new CourseWiseStrReport();
                rpt.Load();
                rpt.SetDataSource(data);
                Stream s = rpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                rpt.Close();
                rpt.Dispose();
                return File(s, "application/pdf");

            }
        }

        public ActionResult CourseWisePaperStrCheckList(string coursecode)
        {


            var ccode = CollegeRepository.GetLoginCollege();

            using (Entities et = new Entities())
            {
                var data = et.Database.SqlQuery<CourseWisePoco>("select * from paperwise_str_new where collegecode = '" + ccode + "' order by coursecode,sub_no ").ToList();



                CourseWisePaperWiseReport rpt = new CourseWisePaperWiseReport();
                rpt.Load();
                rpt.SetDataSource(data);
                Stream s = rpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                rpt.Close();
                rpt.Dispose();
                return File(s, "application/pdf");

            }
        }
    }
}