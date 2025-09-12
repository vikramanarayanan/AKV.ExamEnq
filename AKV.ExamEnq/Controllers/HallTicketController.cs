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
    
 //  [Authorize]
    public class HallTicketController : Controller
    {
        // GET: HallTicket
        public ActionResult Index()
        {
            return View();
        }



        public ActionResult HallTicketReport(string coursecode)
        {
            List<HallTicketPoco> data = new List<HallTicketPoco>();
            var ccode = CollegeRepository.GetLoginCollege();
            var cname = CollegeRepository.GetLoginCollegeName();

            var coursename = CrsmstRepository.Repository.GetCourseName(coursecode).FirstOrDefault().coursename;
            using (Entities et = new Entities())
            {
                 
        var result = et.Database.SqlQuery<HallTicketPoco>("SELECT to_char(nvl(demand,0))demand, to_char(nvl(paid,0))paid,coursecode,collegecode,regno,regu,rollno,name, to_char(nvl(sem_no,0))sem_no,pap_no,opt,community,medium,mobileno,status sub_no,prac,edate,ses,papname1 FROM VIEW_HALLAFF where collegecode = '" + ccode + "' and coursecode =   '" + coursecode + "' and nvl(paid,0) > 0  order by regu,substr(regno,-3),to_date(edate,'dd/mm/yy'),ses ").ToList();
                string regno = string.Empty;
                int r = 0;
                foreach (var item in result)
                {
                    if (regno != item.regno)
                    {
                        r = 1;
                    }
                    else
                    {
                        r = 0;
                    }
                        regno = item.regno;
                    
                    data.Add(new HallTicketPoco
                    {
                        collegecode = ccode,
                        collegename = ccode + "-" + cname,
                        community = item.community,
                        coursecode = coursename,
                        regno = item.regno,
                        rollno = item.rollno,
                        medium = item.medium,
                        mobileno = item.mobileno,
                        name = item.name,
                        pap_no = item.pap_no,
                        papname1 = item.papname1,
                        demand = item.demand,
                        paid = item.paid,
                        sem_no = item.sem_no,
                        coursename = coursename,
                        edate = item.edate,
                        ses = item.ses,
                        photo = r == 1 ? photo(item.coursecode,item.regno) : null,
                        qrcode = null 


                    });
                }

            }
            HallTicketReport rpt = new HallTicketReport();
            rpt.Load();
            rpt.SetDataSource(data);
            Stream s = rpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            rpt.Close();
            rpt.Dispose();
            return File(s, "application/pdf");
        }
     
        private byte[] photo(string coursecode, string regno)
        {
            byte[] t = null;
            using (Entities et = new Entities())
            {
                var photo = et.Database.SqlQuery<poto>("select regno,photo as image from roll where coursecode =  '" + coursecode + "' and regno = '" + regno + "' ");

                foreach (var item in photo)
                {
                    t = item.image;
                }
            }


            return t;

        }
        public class poto
        {
            public string regno { get; set; }
            public byte[] image { get; set; }

        }
    }
}