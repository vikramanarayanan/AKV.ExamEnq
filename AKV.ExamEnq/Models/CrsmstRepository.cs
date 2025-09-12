using AKV.ExanEnq.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AKV.ExamEnq.Models
{
    public class CrsmstRepository
    {
        public static CrsmstRepository Repository = new CrsmstRepository();
        public IEnumerable<CrsmstViewModel> GetCourse()
        {
            var list = new List<CrsmstViewModel>();
            var result = new Entities().CRSMSTs.ToList().Select(t => 
            new CrsmstViewModel {  coursecode = t.COURSECODE, coursename = t.COURSENAME }).Where(x=> !x.coursecode.Contains("REG")).OrderBy(x=> x.coursecode);

            foreach (var item in result)
            {

                list.Add(new CrsmstViewModel {  coursecode = item.coursecode,  coursename = item.coursecode + "-" + item.coursename });
            }


            return list;
        }

        public IEnumerable<CrsmstViewModel> GetCourseName(string course)
        {
            var list = new List<CrsmstViewModel>();
            var result = new Entities().CRSMSTs.Where(x=> x.COURSECODE == course).ToList().Select(t =>
            new CrsmstViewModel { coursecode = t.COURSECODE, coursename = t.COURSENAME });

            foreach (var item in result)
            {

                list.Add(new CrsmstViewModel { coursecode = item.coursecode,  coursename = item.coursecode+"-"+ item.coursename });
            }


            return list;
        }
    }
}