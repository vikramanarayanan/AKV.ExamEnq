using AKV.ExamEnq.Models;
using AKV.ExanEnq.Entity;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AKV.ExamEnq.Controllers
{
    [AllowCrossSiteJson]
    public class ApiController : Controller
    {
        // GET: ApiRoll
        public ActionResult Index()
        {
            return View();
        }
        public class EnrollApplicationPoco
        {
            public string coursecode { get; set; }
            public string coursename { get; set; }
            public string collegecode { get; set; }
            public string collegename { get; set; }
            public string name { get; set; }
            public string regno { get; set; }
            public string sem_no { get; set; }
            public string rollno { get; set; }
            public double demand { get; set; }
            public byte[] photo { get; set; }
            public string photo1 { get; set; }
            public List<PaperPoco> papers { get; set; } = null;

        }

        public class EnrollPoco
        {
            public string coursecode { get; set; }
            public string coursename { get; set; }
            public string collegecode { get; set; }
            public string collegename { get; set; }
            public string sem_no { get; set; }
            public string name { get; set; }
            public string regno { get; set; }
            public string rollno { get; set; }
            public double demand { get; set; }
            public byte[] photo { get; set; }
            public string photo1 { get; set; }

        }
        public class PaperPoco
        {
           
            public string sem_no { get; set; }
            public string pap_no { get; set; }
            public string papname { get; set; }
            public int papfee { get; set; }

            
        }
        public JsonResult GetEnroll(string regno)
        {
            List<EnrollApplicationPoco> appdata = new List<EnrollApplicationPoco>();
            EnrollPoco rolres = new EnrollPoco();
            List<PaperPoco> papres = new List<PaperPoco>();
            using (Entities et = new Entities())
            {
                //November Exam
                //rolres = et.Database.SqlQuery<EnrollPoco>($"select coursecode,coursename,collegecode,collegename,sem_no,regno ,rollno,name,nvl(demand,0)demand,photo from view_api_roll where ( rollno = '{regno}' or regno= '{regno}') and sem_no = '1' ").FirstOrDefault();
                //papres = et.Database.SqlQuery<PaperPoco>($"select ROWNUM as slno,sem_no,pap_no,papname,papfee from view_api_papers where ( rollno = '{regno}' or regno= '{regno}')  ").ToList();
                //May Exam
                rolres = et.Database.SqlQuery<EnrollPoco>($"select coursecode,coursename,collegecode,collegename,sem_no,regno ,rollno,name,nvl(demand,0)demand,photo from view_api_roll where ( regno= '{regno}')  ").FirstOrDefault();
                papres = et.Database.SqlQuery<PaperPoco>($"select ROWNUM as slno,sem_no,pap_no,papname,papfee from view_api_papers where (  regno= '{regno}')  ").ToList();
            }
            if (rolres != null)
            {
                rolres.photo1 = rolres.photo!= null ? Convert.ToBase64String(rolres.photo): null;
                rolres.photo = null;
                appdata.Add(new EnrollApplicationPoco
                {
                    collegecode = rolres.collegecode,
                    collegename = rolres.collegename,
                    coursecode = rolres.coursecode,
                    coursename = rolres.coursename,
                    demand = rolres.demand,
                    name = rolres.name,
                    papers = papres!= null ? papres: null,
                    photo1 = rolres.photo1,
                    regno = rolres.regno,
                    sem_no = rolres.sem_no, 
                    rollno = rolres.rollno

                });
            }
            
            
                return Json(appdata, JsonRequestBehavior.AllowGet);
        }
        
            public JsonResult Getcondonation(string regno)
        {
            List<EnrollApplicationPoco> appdata = new List<EnrollApplicationPoco>();
            EnrollPoco rolres = new EnrollPoco();
            List<PaperPoco> papres = new List<PaperPoco>();
            using (Entities et = new Entities())
            {
                rolres = et.Database.SqlQuery<EnrollPoco>($"select coursecode,coursename,collegecode,collegename,sem_no,regno,rollno,name,demand,photo from view_api_condonation where (rollno = '{regno}') ").FirstOrDefault();
                 
            }
            if (rolres != null)
            {
                rolres.photo1 = rolres.photo != null ? Convert.ToBase64String(rolres.photo) : null;
                rolres.photo = null;
                appdata.Add(new EnrollApplicationPoco
                {
                    collegecode = rolres.collegecode,
                    collegename = rolres.collegename,
                    coursecode = rolres.coursecode,
                    coursename = rolres.coursename,
                    demand = rolres.demand,
                    name = rolres.name,
                    papers = papres != null ? papres : null,
                    photo1 = rolres.photo1,
                    regno = rolres.regno,
                    sem_no = rolres.sem_no,
                    rollno = rolres.rollno

                });
            }


            return Json(appdata, JsonRequestBehavior.AllowGet);
        }
    }
}