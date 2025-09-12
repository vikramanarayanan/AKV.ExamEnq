using AKV.ExamEnq.Models;
using AKV.ExamEnq.Report;
using AKV.ExanEnq.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AKV.ExamEnq.Controllers
{
     [Authorize]
 // [Authorize(Users = "tryvikraman@gmail.com")]

    public class MarkReportController : Controller
    {
        // GET: MarkReport
       //  [Authorize]
        [Authorize(Users = "tryvikraman@gmail.com")]
        public ActionResult InternalMarkReport()
        {
            return View();
        }
      [Authorize(Users = "tryvikraman@gmail.com")]
        public ActionResult InternalMarkReguReport()
        {
            return View();
        }
        [Authorize(Users = "tryvikraman@gmail.com")]
        public ActionResult TheoryMarkReport()
        {
            return View();
        }
        [Authorize(Users = "tryvikraman@gmail.com")]
        public ActionResult AbsentReport()
        {
            return View();
        }
        [Authorize(Users = "tryvikraman@gmail.com")]
        public ActionResult ParacticalMarkReport()
        {
            return View();
        }
        [Authorize(Users = "tryvikraman@gmail.com")]
        public ActionResult ParacticalMarkReguReport()
        {
            return View();
        }
        [Authorize]
        public ActionResult GallySheetReport()
        {
            return View();
        }
        [Authorize(Users = "tryvikraman@gmail.com")]
        public ActionResult GallySheetReport1()
        {
            return View("GallySheetReport");
        }
        [Authorize(Users = "tryvikraman@gmail.com")]
        public ActionResult AttendanceReport()
        {
            return View();
        }
        [Authorize(Users = "tryvikraman@gmail.com")]
        public ActionResult LegendReport()
        {
            return View();
        }
        [Authorize]
        public ActionResult AttenanceReportDownload(string coursecode)
        {


            var ccode = CollegeRepository.GetLoginCollege();

            using (Entities et = new Entities())
            {
                var data = et.Database.SqlQuery<MarkPoco>("select * from view_attendance where collegecode = '" + ccode + "' and coursecode = '" + coursecode + "'  order by sem_no,substr(regno,-3) ").ToList();

                Attendance rpt = new Attendance();
                rpt.Load();
                rpt.SetDataSource(data);
                Stream s = rpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                rpt.Close();
                rpt.Dispose();
                return File(s, "application/pdf");

            }
        }
        public ActionResult LegendDownloadReport(string coursecode)
        {


            var ccode = CollegeRepository.GetLoginCollege();

            using (Entities et = new Entities())
            {
                var data = et.Database.SqlQuery<LegendPoco>("select '" + ccode + "' as collegecode, coursecode,sem_no,f_coursename(coursecode)coursename,f_collegename('" + ccode + "')collegename,regu,sub_credit,(case when opt is not null then (pap_no||'-'||opt) else pap_no end) as pap_no,opt,papname1,passmax,passmin,passmin_Ext,ext,ses,sub_no from  submstmain_Aff  where coursecode = '" + coursecode + "'   order by sub_no,opt ").ToList();



                Legend rpt = new Legend();
                rpt.Load();
                rpt.SetDataSource(data);
                Stream s = rpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                rpt.Close();
                rpt.Dispose();
                return File(s, "application/pdf");

            }
        }
        
        public ActionResult GallyReport(string coursecode)
        {


            var ccode = CollegeRepository.GetLoginCollege();

            using (Entities et = new Entities())
            {
                var data = et.Database.SqlQuery<MarkPoco>("select mthyr,faculty,coursecode,f_coursename(coursecode)coursename,f_collegename(collegecode)collegename,name,regu,sem_no,collegecode,rollno,regno,(case when opt is not null then (pap_no||'-'||opt) else pap_no end) as pap_no,part,sub_credit,sub_no,nvl(ext,0)+nvl(ext_mod,0)ext,ses,status,total,ltgrd,grd_pt, cdt_earned, result from gallysheet where collegecode = '" + ccode + "' and coursecode = '" + coursecode + "'   order by   substr(regno, -3) asc,sub_no ").ToList();



               GallySheet rpt = new GallySheet();
                rpt.Load();
                rpt.SetDataSource(data);
                Stream s = rpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                rpt.Close();
                rpt.Dispose();
                return File(s, "application/pdf");

            }
        }

        public ActionResult AbsentReportData(string coursecode,string pap_no)
        {


            var ccode = CollegeRepository.GetLoginCollege();

            using (Entities et = new Entities())
            {
                var data = et.Database.SqlQuery<AbsentPoco>("select * from view_absent where status = 'Absent' and collegecode = '" + ccode + "' and coursecode = '" + coursecode + "' and pap_no = '" + pap_no + "' order by sem_no,substr(regno,-3)").ToList();

                AbsentReport rpt = new AbsentReport();
                rpt.Load();
                rpt.SetDataSource(data);
                Stream s = rpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                rpt.Close();
                rpt.Dispose();
                return File(s, "application/pdf");

            }
        }
        public ActionResult InternalReport(string coursecode)
        {


            var ccode = CollegeRepository.GetLoginCollege();

            using (Entities et = new Entities())
            {
                // var data = et.Database.SqlQuery<MarkPoco>("select * from view_mark where collegecode = '" + ccode + "' and coursecode = '" + coursecode + "' and  sem_no in (select sem_no from roll where view_mark.regno = regno ) order by sem_no,substr(regno,-3) ").ToList();
                var data = et.Database.SqlQuery<MarkPoco>("select * from view_mark where collegecode = '" + ccode + "' and coursecode = '" + coursecode + "'  order by sem_no,substr(regno,-3) ").ToList();
                InternalMarkReport rpt = new InternalMarkReport();
                rpt.Load();
                rpt.SetDataSource(data);
                Stream s = rpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                rpt.Close();
                rpt.Dispose();
                return File(s, "application/pdf");

            }
        }
        public ActionResult InternalReguReport(string coursecode)
        {


            var ccode = CollegeRepository.GetLoginCollege();

            using (Entities et = new Entities())
            {
                var data = et.Database.SqlQuery<MarkPoco>("select * from view_mark where collegecode = '" + ccode + "' and coursecode = '" + coursecode + "'   order by pap_no,rollno ").ToList();

                InternalMarkReguReport rpt = new InternalMarkReguReport();
                rpt.Load();
                rpt.SetDataSource(data);
                Stream s = rpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                rpt.Close();
                rpt.Dispose();
                return File(s, "application/pdf");

            }
        }
        public ActionResult TheoryReport(string coursecode)
        {


            var ccode = CollegeRepository.GetLoginCollege();

            using (Entities et = new Entities())
            {
                var data = et.Database.SqlQuery<MarkPoco>("select * from view_mark where collegecode = '" + ccode + "' and coursecode = '" + coursecode + "' and pap_no not in (select pap_no from submst where coursecode =   '" + coursecode + "' and prac= 'P')   order by sem_no,name ").ToList();



              TheoryMarkReport rpt = new TheoryMarkReport();
                rpt.Load();
                rpt.SetDataSource(data);
                Stream s = rpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                rpt.Close();
                rpt.Dispose();
                return File(s, "application/pdf");

            }
        }
        public ActionResult PracticalReport(string coursecode)
        {


            var ccode = CollegeRepository.GetLoginCollege();

            using (Entities et = new Entities())
            {
                var data = et.Database.SqlQuery<MarkPoco>("select * from view_mark where collegecode = '" + ccode + "' and coursecode = '" + coursecode + "'  and SUBSTR(pap_no,1,9) in (select pap_no from submst where coursecode =   '" + coursecode + "' and prac in ('P','V') ) order by pap_no,substr(regno,-3)  ").ToList();



              PracticalMarkReport rpt = new PracticalMarkReport();
                rpt.Load();
                rpt.SetDataSource(data);
                Stream s = rpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                rpt.Close();
                rpt.Dispose();
                return File(s, "application/pdf");

            }
        }
        public ActionResult PracticalReguReport(string coursecode)
        {


            var ccode = CollegeRepository.GetLoginCollege();

            using (Entities et = new Entities())
            {
                var data = et.Database.SqlQuery<MarkPoco>("select * from view_mark where collegecode = '" + ccode + "' and coursecode = '" + coursecode + "'  and pap_no  in (select pap_no from submst where  coursecode = '" + coursecode + "' and prac = 'P' ) order by pap_no,rollno ").ToList();



                PracticalMarkReguReport rpt = new PracticalMarkReguReport();
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