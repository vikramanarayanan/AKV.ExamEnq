using AKV.ExanEnq.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace AKV.ExamEnq.Models
{
    public class MarkRepository
    {
        public static MarkRepository Repository = new MarkRepository();



        public IEnumerable<MarkViewModel> GetInternalPapersByCourse(string course, string namefilter)
        {
            var ccode = CollegeRepository.GetLoginCollege();
            List<MarkViewModel> list = new List<MarkViewModel>();
            using (Entities et = new Entities())
            {
                
                if (!string.IsNullOrEmpty(namefilter))
                {
                    //string q = "select distinct pap_no,(pap_no ||' '||f_papname_withopt(coursecode,pap_no,regu)) papname,F_SESMAX(COURSECODE,PAP_NO,REGU) sesmax,F_EXTMAX(COURSECODE,PAP_NO,REGU) extmax from mark where collegecode = '" + ccode + "' and coursecode =   '" + course + "'    and (pap_no ||' '||f_papname_withopt(coursecode,pap_no,regu)) like upper('%" + namefilter + "%') order by substr(pap_no,-2) ";
                    string q = "select distinct pap_no,(pap_no ||' '||f_papname_withopt(coursecode,pap_no,regu)) papname,F_SESMAX(COURSECODE,PAP_NO,REGU) sesmax,F_EXTMAX(COURSECODE,PAP_NO,REGU) extmax from mark where collegecode = '" + ccode + "' and coursecode =   '" + course + "'  and sem_no in (select sem_no from roll where mark.regno = regno )  and (pap_no ||' '||f_papname_withopt(coursecode,pap_no,regu)) like upper('%" + namefilter + "%') order by substr(pap_no,-2) ";
                    var res = et.Database.SqlQuery<MarkViewModel>(q).ToList();
                    //result = result.Where(x => x.c_name.ToLower().Contains(namefilter.ToLower()));
                    foreach (var item in res)
                    {
                        list.Add(new MarkViewModel { pap_no = item.pap_no, papname = item.papname, sesmax = item.sesmax, extmax = item.extmax  });
                    }
                }
                else
                {
                    var res = et.Database.SqlQuery<MarkViewModel>("select distinct pap_no,(pap_no ||' '||f_papname_withopt(coursecode,pap_no,regu)) papname,F_SESMAX(COURSECODE,PAP_NO,REGU) sesmax,F_EXTMAX(COURSECODE,PAP_NO,REGU) extmax from mark where collegecode = '" + ccode + "' and coursecode =   '" + course + "'  and sem_no in (select sem_no from roll where mark.regno = regno ) order by  substr(pap_no,-2) ").ToList();
                   // var res = et.Database.SqlQuery<MarkViewModel>("select distinct pap_no,(pap_no ||' '||f_papname_withopt(coursecode,pap_no,regu)) papname,F_SESMAX(COURSECODE,PAP_NO,REGU) sesmax,F_EXTMAX(COURSECODE,PAP_NO,REGU) extmax from mark where collegecode = '" + ccode + "' and coursecode =   '" + course + "'   order by  substr(pap_no,-2) ").ToList();
                    foreach (var item in res)
                    {
                        list.Add(new MarkViewModel { pap_no = item.pap_no, papname = item.papname, sesmax = item.sesmax, extmax = item.extmax });
                    }
                }

               
               
               

            }
             


            return list;
            
        }

        public IEnumerable<MarkViewModel> GetTheoryPapersByCourse(string course, string namefilter)
        {
            var ccode = CollegeRepository.GetLoginCollege();
            List<MarkViewModel> list = new List<MarkViewModel>();
            using (Entities et = new Entities())
            {

                if (!string.IsNullOrEmpty(namefilter))
                {
                    string q = "select distinct pap_no,(pap_no ||' '||f_papname_withopt(coursecode,pap_no,regu)) papname,F_SESMAX(COURSECODE,PAP_NO,regu) sesmax,F_EXTMAX(COURSECODE,PAP_NO,regu) extmax from mark where collegecode = '" + ccode + "' and coursecode =   '" + course + "' and pap_no not in (select pap_no from submst where coursecode =   '" + course + "' and prac= 'P')  and (pap_no ||' '||f_papname_withopt(coursecode,pap_no,regu)) like upper('%" + namefilter + "%') order by pap_no ";
                    var res = et.Database.SqlQuery<MarkViewModel>(q).ToList();
                    //result = result.Where(x => x.c_name.ToLower().Contains(namefilter.ToLower()));
                    foreach (var item in res)
                    {
                        list.Add(new MarkViewModel { pap_no = item.pap_no, papname = item.papname, sesmax = item.sesmax, extmax = item.extmax });
                    }
                }
                else
                {
                    var res = et.Database.SqlQuery<MarkViewModel>("select distinct pap_no,(pap_no ||' '||f_papname_withopt(coursecode,pap_no,regu)) papname,F_SESMAX(COURSECODE,PAP_NO,regu) sesmax,F_EXTMAX(COURSECODE,PAP_NO,regu) extmax from mark where collegecode = '" + ccode + "' and coursecode =   '" + course + "' and pap_no not in (select pap_no from submst where coursecode =   '" + course + "' and prac= 'P')   order by pap_no ").ToList();
                    foreach (var item in res)
                    {
                        list.Add(new MarkViewModel { pap_no = item.pap_no, papname = item.papname, sesmax = item.sesmax, extmax = item.extmax });
                    }
                }





            }



            return list;

        }
        public IEnumerable<MarkViewModel> GetPracticalPapersByCourse(string course, string namefilter)
        {
            var ccode = CollegeRepository.GetLoginCollege();
            List<MarkViewModel> list = new List<MarkViewModel>();
            using (Entities et = new Entities())
            {
                if (course.Contains("REG"))
                {
                    if (!string.IsNullOrEmpty(namefilter))
                    {
                        string q = "select distinct pap_no,(pap_no ||' '||f_papname_withopt(coursecode,pap_no,regu)) papname,F_SESMAX(COURSECODE,PAP_NO,regu) sesmax,F_EXTMAX(COURSECODE,PAP_NO,regu) extmax from mark where collegecode = '" + ccode + "' and coursecode =   '" + course + "' and substr(pap_no,1,9) in (select pap_no from submst where coursecode =   '" + course + "' and prac= 'P')  and (pap_no ||' '||f_papname_withopt(coursecode,pap_no,regu)) like upper('%" + namefilter + "%') order by pap_no ";
                        var res = et.Database.SqlQuery<MarkViewModel>(q).ToList();
                        //result = result.Where(x => x.c_name.ToLower().Contains(namefilter.ToLower()));
                        foreach (var item in res)
                        {
                            list.Add(new MarkViewModel { pap_no = item.pap_no, papname = item.papname, sesmax = item.sesmax, extmax = item.extmax });
                        }
                    }
                    else
                    {
                        var res = et.Database.SqlQuery<MarkViewModel>("select distinct pap_no,(pap_no ||' '||f_papname_withopt(coursecode,pap_no,regu)) papname,F_SESMAX(COURSECODE,PAP_NO,regu) sesmax,F_EXTMAX(COURSECODE,PAP_NO,regu) extmax from mark where collegecode = '" + ccode + "' and coursecode =   '" + course + "' and substr(pap_no,1,9) in (select pap_no from submst where coursecode =   '" + course + "' and prac= 'P')  order by pap_no ").ToList();
                        foreach (var item in res)
                        {
                            list.Add(new MarkViewModel { pap_no = item.pap_no, papname = item.papname, sesmax = item.sesmax, extmax = item.extmax });
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(namefilter))
                    {
                        string q = "select distinct pap_no,(pap_no ||' '||f_papname_withopt(coursecode,pap_no,regu)) papname,F_SESMAX(COURSECODE,PAP_NO,regu) sesmax,F_EXTMAX(COURSECODE,PAP_NO,regu) extmax from mark where collegecode = '" + ccode + "' and coursecode =   '" + course + "'   and substr(pap_no,1,9) in (select pap_no from submst where coursecode =   '" + course + "' and prac IN ('P','V'))  and (pap_no ||' '||f_papname_withopt(coursecode,pap_no,regu)) like upper('%" + namefilter + "%') order by pap_no ";
                        var res = et.Database.SqlQuery<MarkViewModel>(q).ToList();
                        //result = result.Where(x => x.c_name.ToLower().Contains(namefilter.ToLower()));
                        foreach (var item in res)
                        {
                            list.Add(new MarkViewModel { pap_no = item.pap_no, papname = item.papname, sesmax = item.sesmax, extmax = item.extmax });
                        }
                    }
                    else
                    {
                        var res = et.Database.SqlQuery<MarkViewModel>("select distinct pap_no,(pap_no ||' '||f_papname_withopt(coursecode,pap_no,regu)) papname,F_SESMAX(COURSECODE,PAP_NO,regu) sesmax,F_EXTMAX(COURSECODE,PAP_NO,regu) extmax from mark where collegecode = '" + ccode + "'    and coursecode =   '" + course + "' and substr(pap_no,1,9) in (select pap_no from submst where coursecode =   '" + course + "' and prac IN ('P','V'))  order by pap_no ").ToList();
                        foreach (var item in res)
                        {
                            list.Add(new MarkViewModel { pap_no = item.pap_no, papname = item.papname, sesmax = item.sesmax, extmax = item.extmax });
                        }
                    }


                }


            }



            return list;

        }
        public void Inttag(string course,string pap_no)
        {
            using (Entities et = new Entities())
            {
                var ccode = CollegeRepository.GetLoginCollege();

                var res = et.Database.ExecuteSqlCommand("update mark set int_view = '1' where nvl(ses,0) > 0 and  collegecode = '" + ccode + "' and coursecode =   '" + course + "' and pap_no =   '" + pap_no + "' ");
                
            }
             
        }
        public void Practag(string course, string pap_no)
        {
            using (Entities et = new Entities())
            {
                var ccode = CollegeRepository.GetLoginCollege();

                var res = et.Database.ExecuteSqlCommand("update mark set prac_view = '1' where nvl(ext,0) > 0 and  collegecode = '" + ccode + "' and coursecode =   '" + course + "' and pap_no =   '" + pap_no + "' ");
                
            }

        }
        public IEnumerable<MarkViewModel> GetMark_Internal(string course, string papno)
        {
            var ccode = CollegeRepository.GetLoginCollege();
            List<MarkViewModel> list = new List<MarkViewModel>();
            using (Entities et = new Entities())
            {

                //  var res = et.Database.SqlQuery<MarkViewModel>("select id,coursecode,rollno,regno,f_name_regno(collegecode,coursecode,regno)name,pap_no,ses,ext from mark where collegecode = '" + ccode + "' and coursecode =   '" + course + "' and pap_no =   '" + papno + "' and int_view is null and tag = '1' and sem_no in (select sem_no from roll where mark.regno = regno )  order by sem_no,substr(regno,-3) ").ToList();
                var res = et.Database.SqlQuery<MarkViewModel>("select id,coursecode,rollno,regno,f_name_regno(collegecode,coursecode,regno)name,pap_no,ses,ext from mark where collegecode = '" + ccode + "' and coursecode =   '" + course + "' and pap_no =   '" + papno + "' and int_view is null    order by sem_no,substr(regno,-3) ").ToList();
                foreach (var item in res)
                    {
                        list.Add(new MarkViewModel { id = item.id, coursecode = item.coursecode,rollno = item.rollno,regno = item.regno, name = item.name, pap_no = item.pap_no,ses= item.ses,ext = item.ext });
                    }
                

            }



            return list;

        }
        public IEnumerable<MarkViewModel> GetMark_Practical(string course, string papno)
        {
            var ccode = CollegeRepository.GetLoginCollege();
            List<MarkViewModel> list = new List<MarkViewModel>();
            using (Entities et = new Entities())
            {

                var res = et.Database.SqlQuery<MarkViewModel>("select id,coursecode,rollno,regno,f_name_regno(collegecode,coursecode,regno)name,pap_no,ses,ext from mark where collegecode = '" + ccode + "' and coursecode =   '" + course + "' and pap_no =   '" + papno + "' and prac_view is null  order by sem_no,substr(regno,-3) ").ToList();
                foreach (var item in res)
                {
                    list.Add(new MarkViewModel { id = item.id, coursecode = item.coursecode, rollno = item.rollno, regno = item.regno, name = item.name, pap_no = item.pap_no, ses = item.ses, ext = item.ext });
                }


            }



            return list;

        }

        public void UpdateSesMark(MarkViewModel mark)
        {
            using (Entities nip = new Entities())
            {
                var entity = nip.MARKs.Where(x => x.ID.Equals(mark.id)).FirstOrDefault();

                if (entity != null)
                {
                     
                    entity.SES = mark.ses;


                    nip.SaveChanges();
                }
            }
        }


        public void UpdateExtMark(MarkViewModel mark)
        {
            using (Entities nip = new Entities())
            {
                var entity = nip.MARKs.Where(x => x.ID.Equals(mark.id)).FirstOrDefault();

                if (entity != null)
                {

                    entity.EXT = mark.ext;


                    nip.SaveChanges();
                }
            }
        }



    }
}