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
    public class AppEntryController : Controller
    {
        // GET: AppEntry
       [Authorize]
     //  [Authorize(Users = "admin@aucoeexam.in")]
        public ActionResult AppEntry()
        {
            return View();
        }
        public ActionResult AppCheckList(string coursecode)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            List<RollPoco> data = new List<RollPoco>();
            var ccode = CollegeRepository.GetLoginCollege();
            var cname = CollegeRepository.GetLoginCollegeName();

            var coursename = CrsmstRepository.Repository.GetCourseName(coursecode).FirstOrDefault().coursename;
            using (Entities et = new Entities())
            {

                //var result = et.Database.SqlQuery<ROLL>(" SELECT coursecode, f_coursename(coursecode)coursename,collegecode, name, sex, community, medium,   f_papers(collegecode, coursecode, rollno)  as papers,rollno,regno,photo,id,mobileno,status,arrear,rstatus,remark,nvl(examfee,0) examfee ,paidornot,regu  from roll where collegecode = '" + ccode + "' and coursecode =   '" + coursecode + "' order by substr(rollno,-3) asc ").ToList();
                var result = et.Database.SqlQuery<ROLL>("SELECT coursecode, f_coursename(coursecode)coursename,collegecode, name, sex, community, medium,   f_papers(collegecode, coursecode, regno)  as papers,rollno,regno,photo,id,mobileno,status,arrear,rstatus,remark,nvl(examfee,0) examfee ,paidornot,regu,sem_no,demand,paid,due,UNIQUEREFNUMBER,TRANSACTIONDATE  from roll where collegecode = '" + ccode + "' and coursecode =   '" + coursecode + "' and sem_no is not null order by sem_no,substr(regno,-3) asc ").ToList();

                foreach (var item in result)
                {
                    data.Add(new RollPoco
                    {
                        collegecode = ccode,
                        collegename = ccode + "-" + cname,
                        community = item.COMMUNITY,
                        coursecode = coursename,
                        regno = item.REGNO,
                        medium = item.MEDIUM,
                        mobileno = item.MOBILENO,
                        name = item.NAME,
                        papers = (!string.IsNullOrEmpty(item.PAPERS)) ? item.PAPERS.Replace("|", ", ") : string.Empty,
                        rollno = item.ROLLNO,
                        sex = item.SEX,
                        status = item.STATUS,
                        rstatus = item.RSTATUS,
                        examfee=(decimal)item.EXAMFEE,
                        paidornot = item.PAIDORNOT,
                        regu = item.REGU
                      
                      


                    });
                }

            }
            ApplicationCheckList rpt = new ApplicationCheckList();
            rpt.Load();
            rpt.SetDataSource(data);
            Stream s = rpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            rpt.Close();
            rpt.Dispose();
            return File(s, "application/pdf");
        }
        public ActionResult Edit()
        {
            var id = Request["id"];
           
            TempData["id"] = id;
           
            //return PartialView("index1");
            return View();
        }
    }
}