using AKV.ExanEnq.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
 

namespace AKV.ExamEnq.Models
{
    public class CollegeRepository
    {

        public static CollegeRepository Repository = new CollegeRepository();
        public IEnumerable<CollegeViewModel> GetCollege()
        {
            var list = new List<CollegeViewModel>();
            var result = new Entities().COLLEGEs.ToList().Select(t => new CollegeViewModel {   collegecode= t.COLLEGECODE,  collegename = t.ADDR1  });

            foreach (var item in result)
            {

                list.Add(new CollegeViewModel { collegecode = item.collegecode, collegename = item.collegename});
            }


            return list;
        }
        public static string GetLoginCourse()
        {
            var ccode = HttpContext.Current.User.Identity.Name.Substring(0, 3);
            if (ccode == "REG")
            {
                return HttpContext.Current.User.Identity.Name.Substring(0, 6);
            }
            return ccode;
        }

        public static string GetLoginCollege()
        {
            var ccode = HttpContext.Current.User.Identity.Name.Substring(0, 3);
            if (ccode == "REG")
            {
                ccode = "000";
            }
            return ccode;
        }

        public static string GetLoginCollegeName()
        {
           var ccode = HttpContext.Current.User.Identity.Name.Substring(0, 3);
            if(ccode == "REG")
            {
                ccode = "000";
            }
            var res = new Entities().COLLEGEs.Where(x => x.COLLEGECODE == ccode).FirstOrDefault();
            var result =  res.ADDR1+"-"+res.ADDR6 ;
            return result;
        }
    }
}