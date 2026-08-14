
using AKV.ExamEnq.Models;
using AKV.ExamEnq.Report;
using AKV.ExanEnq.Entity;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Affiliated.Controllers
{
    [Authorize]

   // [Authorize(Users = "tryvikraman@gmail.com")]

    public class RollGridController : Controller
    {

        public ActionResult RollCheckList(string coursecode)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            List<RollPoco> data = new List<RollPoco>();
            var ccode = CollegeRepository.GetLoginCollege();
            var cname = CollegeRepository.GetLoginCollegeName();

            var coursename = CrsmstRepository.Repository.GetCourseName(coursecode).FirstOrDefault().coursename;
            using (Entities et = new Entities())
            {
                
                var result =  et.Database.SqlQuery<ROLL>(" SELECT coursecode, collegecode, name, sex, community, medium, f_papers(collegecode, coursecode, rollno) as papers,photo,rollno,regno,id,mobileno,status,arrear,rstatus,remark,nvl(examfee,0)examfee,paidornot,regu,sem_no,demand,paid,due,UNIQUEREFNUMBER,TRANSACTIONDATE   from roll where collegecode = '" + ccode + "' and coursecode =   '" + coursecode + "' and regu = 'B' and sem_no is null order by substr(rollno,-3) asc ").ToList();
                //et.ROLLs.Where(x=> x.COLLEGECODE == ccode && x.COURSECODE == coursecode ).ToList();
                
        
        
                foreach (var item in result)
                {
                  data.Add(new RollPoco
                { 
                       collegecode = ccode,
                    collegename = ccode + "-" + cname,
                    community = item.COMMUNITY, 
                     coursecode =  coursename,
                      photo = item.PHOTO,
                       medium = item.MEDIUM,
                        mobileno = item.MOBILENO,
                         name = item.NAME,
                          papers = (!string.IsNullOrEmpty(item.PAPERS)) ? item.PAPERS.Replace("|", ", ")  : string.Empty,
                           rollno = item.ROLLNO,
                            sex = item.SEX,
                             status = item.STATUS,
                             examfee = (decimal)item.EXAMFEE,
                             paidornot = item.PAIDORNOT,
                             sem_no = item.SEM_NO.ToString()
                   

                });
                }
                
            }
                RollCheckListReport rpt = new RollCheckListReport();
            rpt.Load();
            rpt.SetDataSource(data);
            Stream s = rpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            rpt.Close();
            rpt.Dispose();
            return File(s, "application/pdf");
        }

        // GET: RollGrid
       [Authorize(Users = "tryvikraman@gmail.com")]
     //  [Authorize]
        public ActionResult Index()
        {
            // TempData["id"] = Guid.NewGuid().ToString();
          
            return View();
        }

        
        public ActionResult Index1()
        {
            var id = Request["id"];
            TempData["id"] = id;
            //return PartialView("index1");
            return View();
        }
        public class vv
        {
            public bool success { get; set; }
        }
        [HttpPost]
        public JsonResult AppSave1( string gender,string  medium, string category,  string mobileno )
        {


            var id = TempData["id"] as string;
            TempData["id"] = id;
            using (Entities et = new Entities())
            {
                var entity = et.ROLLs.Where(x => x.ID == id).FirstOrDefault();
                    if (entity != null)
                    { 
                    entity.SEX = gender;
                    entity.MEDIUM = medium;
                    entity.COMMUNITY = category;
                      entity.MOBILENO = mobileno; 

                    }
                et.SaveChanges();
            }
            return Json(new vv { success = true });

        }
        public JsonResult AppSave2(List<SubmstViewModel> papers)
        //public JsonResult AppSave2(List<SubmstViewModel> papers,string feepaid,decimal examfee)
        {
            var id = TempData["id"] as string;
            TempData["id"] = id;
            using (Entities et = new Entities())
            {
                var entity = et.ROLLs.Where(x => x.ID == id).FirstOrDefault();
                var mpapers = string.Empty;
                if (papers != null)
                {
                    foreach (var item in papers)
                    {
                        mpapers += item.pap_no + "|";
                    }
                }
                if (entity != null)
                {
                    et.Database.ExecuteSqlCommand("delete from papers where rollno = '" + entity.ROLLNO + "' and coursecode = '"+entity.COURSECODE+"' ");
                    entity.PAPERS = mpapers;
                    entity.EXAMFEE= 0;
                    entity.PAIDORNOT = "No";

                    if (papers != null)
                    {
                        foreach (var item in papers)
                        {
                            et.PAPERS.Add(new PAPER
                            {
                                COURSECODE = entity.COURSECODE,
                                PAPNAME1 = item.papname1,
                                PAP_NO = item.pap_no,
                                ROLLNO = entity.ROLLNO,
                                SEM_NO = item.sem_no,
                                ID = Guid.NewGuid().ToString()
                             
                            });
                        }
                    }
              
                }
                

                et.SaveChanges();

            }
        
            return Json(new vv { success = true });

        }


        public JsonResult ReadByApplno()
        {
             Thread.Sleep(100);

            var id = TempData["id"] as string;
            TempData["id"] = id;
            var result = RollRepository.Repository.GetApplicationByID(id);

              
            return Json(result, JsonRequestBehavior.AllowGet);

        }


        public static Image resizeImage(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
        }
        public Image byteArrayToImage(byte[] source)
        {
            MemoryStream ms = new MemoryStream(source);
            Image ret = Image.FromStream(ms);
            return ret;
        }
        public static Image ResizeImage(Image originalImage, Size targetSize)
        {
            Bitmap resizedBitmap = new Bitmap(targetSize.Width, targetSize.Height);
            using (Graphics graphics = Graphics.FromImage(resizedBitmap))
            {
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                graphics.DrawImage(originalImage, 0, 0, targetSize.Width, targetSize.Height);
            }
            return resizedBitmap;
        }

        public static byte[] ImageToByteArray(Image x)
        {
            ImageConverter _imageConverter = new ImageConverter();
            byte[] xByte = (byte[])_imageConverter.ConvertTo(x, typeof(byte[]));
            return xByte;
        }
         
        [HttpPost]
        public ActionResult PhotoSave(IEnumerable<HttpPostedFileBase> uploadphoto, string guid)
        {
            // The Name of the Upload component is "files"
            byte[] test = null;
            if (uploadphoto != null)
            {

                foreach (var file in uploadphoto)
                {
                    using (Entities et = new Entities())
                    {
                        var id = TempData["id"] as string;
                        TempData["id"] = id;
                        var entity = et.ROLLs.Where(x => x.ID.Equals(id)).FirstOrDefault();
                        if (entity != null)
                        {
                          using (var reader = new System.IO.BinaryReader(file.InputStream))
                            {
                                var image = byteArrayToImage(reader.ReadBytes(file.ContentLength));
                               // var newimage = ResizeImage(image, new Size(200, 220));// resizeImage(image, new Size(100, 120));
                               var imagebyte = ImageToByteArray(image);
                                entity.PHOTO = imagebyte;
                                entity.STATUS = "1";
                                test = imagebyte;
                     
                      
                       
                            }

                            et.SaveChanges();
                        }
                    }

                }
            }

            return Json(new { status = test != null ? Convert.ToBase64String(test) : null }, "text/plain");
            //return Json(new { status = "OK" }, "text/plain");
            // Return an empty string to signify success
            //return Content("");
        }
       
        public ActionResult PhotoRemove(string[] fileNames, string guid)
        {
            // The parameter of the Remove action must be called "fileNames"

            if (fileNames != null)
            {
                foreach (var fullName in fileNames)
                {
                    using (Entities et = new Entities())
                    {
                        var id = TempData["id"] as string;
                        TempData["id"] = id;
                        var entity = et.ROLLs.Where(x => x.ID.Equals(id)).FirstOrDefault();
                        if (entity != null)
                        {
                           entity.PHOTO = null;
                            entity.STATUS = "0";
                            et.SaveChanges();
                        }
                    }
                }
            }

            // Return an empty string to signify success
            return Content("");
        }
        public JsonResult GetSubmst()
        { 
            
            string coursecode = string.Empty;
            string regu = string.Empty;
            int semno = 0;

            using (Entities et = new Entities())
            {
                var id = TempData["id"] as string;
                TempData["id"] = id;
                var result = et.ROLLs.Where(x => x.ID.Equals(id)).FirstOrDefault();
                coursecode =result.COURSECODE;
                if (result.REMARK == "RJ")
                {
                    regu = "RJ";
                }
                else
                {
                    if (string.IsNullOrEmpty(result.REGU))
                    {
                        regu = "B";
                        semno = 1;

                    }
                    else
                    {
                        regu = result.REGU;
                        semno = Convert.ToInt32(result.SEM_NO);
                        if (semno == 0)
                            semno = 1;
                    }
                }

            }
          

            return Json(SubmstRepository.Repository.GetSubmst(coursecode,regu,semno), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetCategoryes()
        {
            return Json(CategoryRepository.Repository.GetCategory(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetCourses()
        {
            var ccode = CollegeRepository.GetLoginCollege();
            var coursecode = CollegeRepository.GetLoginCourse();
            
            if (ccode == "000")
            {
                return Json(CrsmstRepository.Repository.GetCourseName(coursecode), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(CrsmstRepository.Repository.GetCourse(), JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetColleges()
        { 
            return Json(CollegeRepository.Repository.GetCollege(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult ReadRoll([DataSourceRequest] DataSourceRequest request, string coursecode)
        {
            return Json(RollRepository.Repository.GenerateCustomersByrid(coursecode, null).ToDataSourceResult(request));
        }
        public ActionResult ReadRollByRegno([DataSourceRequest] DataSourceRequest request, string coursecode)
        {
            return Json(RollRepository.Repository.GenerateCustomersByridRegnoOrder(coursecode, null).ToDataSourceResult(request));
        }
        public ActionResult RollCreate([DataSourceRequest] DataSourceRequest request, RollViewModel roll)
        {


            if (ModelState.IsValid)
            {
                {


                    RollRepository.Repository.Create(roll);


                }
            }

            return Json(new[] { roll }.ToDataSourceResult(request, ModelState));

        }


        public ActionResult Rollupdate([DataSourceRequest] DataSourceRequest request, RollViewModel roll)
        {
            if (ModelState.IsValid)
            {

                RollRepository.Repository.Update(roll);

            }


            return Json(new[] { roll }.ToDataSourceResult(request, ModelState));
        }


        public ActionResult RollDestroy([DataSourceRequest] DataSourceRequest request, RollViewModel roll)
        {

            if (ModelState.IsValid)
            {
                RollRepository.Repository.Destroy(roll);

            }
            return Json(new[] { roll }.ToDataSourceResult(request, ModelState));
        }

    }
}