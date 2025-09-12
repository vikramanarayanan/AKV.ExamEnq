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
    [Authorize(Users = "tryvikraman@gmail.com")]
   // [Authorize]
    public class RollWithRegnoController : Controller
    {
        // GET: RollWithRegno
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Index1()
        {
            return View();
        }

        public ActionResult RollWithRegnoReport(string coursecode,string ordertag)
        {
            List<RollPoco> data = new List<RollPoco>();
            var ccode = CollegeRepository.GetLoginCollege();
            var cname = CollegeRepository.GetLoginCollegeName();

            var coursename = CrsmstRepository.Repository.GetCourseName(coursecode).FirstOrDefault().coursename;
            using (Entities et = new Entities())
            {
                string sqlstring = string.Empty;
                if(ordertag == "reg")
                {
                    sqlstring = "select * from roll where collegecode = '" + ccode + "' and coursecode =   '" + coursecode + "' order by regno ";
                }
                else
                {
                    sqlstring = "select * from roll where collegecode = '" + ccode + "' and coursecode =   '" + coursecode + "' order by rollno ";
                }
               

                var result = et.Database.SqlQuery<ROLL>(sqlstring).ToList();
                //et.ROLLs.Where(x=> x.COLLEGECODE == ccode && x.COURSECODE == coursecode ).ToList();
                foreach (var item in result)
                {
                    data.Add(new RollPoco
                    {
                        collegecode = ccode,
                        collegename = ccode + "-" + cname,
                        community = item.COMMUNITY,
                        coursecode = item.COURSECODE + "-" + coursename,
                        regno = item.REGNO,
                        medium = item.MEDIUM,
                        mobileno = item.MOBILENO,
                        name = item.NAME,
                        papers = (!string.IsNullOrEmpty(item.PAPERS)) ? item.PAPERS.Replace("|", ", ") : string.Empty,
                        rollno = item.ROLLNO,
                        sex = item.SEX,
                        status = item.STATUS


                    });
                }

            }
            RollWithRegnoReport rpt = new RollWithRegnoReport();
            rpt.Load();
            rpt.SetDataSource(data);
            Stream s = rpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            rpt.Close();
            rpt.Dispose();
            return File(s, "application/pdf");
        }

       
    }
}