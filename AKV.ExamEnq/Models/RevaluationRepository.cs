using AKV.ExanEnq.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AKV.ExamEnq.Models
{
    public class RevaluationRepository
    {
        public static RevaluationRepository Repository = new RevaluationRepository();



        public IEnumerable<AbsentViewModel> GetRevaluiationPapersByCourse(string course, string namefilter)
        {
            var ccode = CollegeRepository.GetLoginCollege();
            List<AbsentViewModel> list = new List<AbsentViewModel>();
            using (Entities et = new Entities())
            {
                
                if (!string.IsNullOrEmpty(namefilter))
                {
                    string q = "select distinct pap_no,(pap_no ||' '||case when opt is not null then  F_PAPNAME_PAPNO(pap_no||'-'||opt) else (f_papname_withopt(coursecode,pap_no,regu)) end)papname ,status from revaluation where collegecode = '" + ccode + "' and coursecode =   '" + course + "'  and (pap_no ||' '||f_papname_withopt(coursecode,pap_no,regu)) like upper('%" + namefilter + "%')   order by pap_no ";
                    var res = et.Database.SqlQuery<AbsentViewModel>(q).ToList();
                    //result = result.Where(x => x.c_name.ToLower().Contains(namefilter.ToLower()));
                    foreach (var item in res)
                    {
                        list.Add(new AbsentViewModel { pap_no = item.pap_no, papname = item.papname,  status = item.status });
                    }
                }
                else
                {
                    var res = et.Database.SqlQuery<AbsentViewModel>("select distinct pap_no,(pap_no ||' '||case when opt is not null then  F_PAPNAME_PAPNO(pap_no||'-'||opt) else (f_papname_withopt(coursecode,pap_no,regu)) end)papname ,status from revaluation where collegecode = '" + ccode + "' and coursecode =   '" + course + "' order by pap_no ").ToList();
                    foreach (var item in res)
                    {
                        list.Add(new AbsentViewModel { pap_no = item.pap_no, papname = item.papname,status = item.status });
                    }
                }

               
               
               

            }
             


            return list;
            
        }

        
        public IEnumerable<AbsentViewModel> GetRevaluation(string course, string papno)
        {
            var ccode = CollegeRepository.GetLoginCollege();
            List<AbsentViewModel> list = new List<AbsentViewModel>();
            using (Entities et = new Entities())
            {
 
                    var res = et.Database.SqlQuery<AbsentViewModel>("select id,coursecode,rollno,regno,name,pap_no,status from revaluation where collegecode = '" + ccode + "' and coursecode =   '" + course + "' and pap_no =   '" + papno + "'  order by regno ").ToList();
                    foreach (var item in res)
                    {
                        list.Add(new AbsentViewModel { id = item.id, coursecode = item.coursecode,rollno = item.rollno,regno = item.regno, name = item.name, pap_no = item.pap_no, status = item.status });
                    }
                

            }



            return list;

        }

       

        public void UpdateRevaluation(AbsentViewModel absent)
        {
            using (Entities nip = new Entities())
            {
                var entity = nip.REVALUATIONs.Where(x => x.ID.Equals(absent.id)).FirstOrDefault();

                if (entity != null)
                {

                    entity.STATUS = absent.status;


                    nip.SaveChanges();
                }
            }
        }



    }
}