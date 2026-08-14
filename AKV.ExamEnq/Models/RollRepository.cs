
using Affiliated.Models;
using AKV.ExanEnq.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace AKV.ExamEnq.Models
{
    public class RollRepository
    {
        public static RollRepository Repository = new RollRepository();


        public RollModel GetApplicationByID(string id)
        {
            var data = new RollModel();
            using (Entities et = new Entities())
            {

                var item = et.ROLLs.Where(x => x.ID == id).FirstOrDefault();

                var coursename = et.CRSMSTs.Where(x => x.COURSECODE == item.COURSECODE).FirstOrDefault().COURSENAME;
                List<SubmstViewModel> sub = new List<SubmstViewModel>();
                string mregno = string.Empty;
                if (string.IsNullOrEmpty(item.REGNO))
                {
                    mregno = item.ROLLNO;
                }
                else
                {
                    mregno = item.REGNO;
                }
                var papers = et.PAPERS.Where(X => (X.ROLLNO == item.ROLLNO || X.ROLLNO == mregno)).ToList();
                foreach (var i in papers)
                {
                    sub.Add(new SubmstViewModel
                    {
                        coursecode =i.COURSECODE,
                         papname1 = i.PAPNAME1, 
                          pap_no = i.PAP_NO,
                           sem_no = i.SEM_NO
                });
                }
              
                if (item != null)
                { 
                    data.coursecode = item.COURSECODE+"-"+coursename;
                    data.name = item.NAME;
                    data.community = item.COMMUNITY;
                    data.sex = item.SEX;
                    data.rollno = item.ROLLNO;
                    data.medium = item.MEDIUM;
                    data.mobileno = item.MOBILENO;
                    data.image = item.PHOTO != null ?Convert.ToBase64String(item.PHOTO): string.Empty;
                    data.papers = sub;
                    data.examfee = item.EXAMFEE;
                    data.paidornot = item.PAIDORNOT;
                    
                    
                    
                    //end             
 

                }


            }
            

            return data;
        }
        public IEnumerable<RollViewModel> GenerateCustomersByrid(string coursecode, string namefilter)
        {
            var ccourse = CollegeRepository.GetLoginCollege();
            var result = new Entities().ROLLs.Where(x=> x.COURSECODE ==coursecode && x.COLLEGECODE == ccourse && string.IsNullOrEmpty(x.RSTATUS)  && string.IsNullOrEmpty(x.RSTATUS) && string.IsNullOrEmpty(x.REGNO) ).ToList().Select(t => new RollViewModel {
                        coursecode = t.COURSECODE,
                         rollno = t.ROLLNO,
                         regno = t.REGNO,
                         id =t.ID,
                          mobileno = t.MOBILENO,
                           name = t.NAME,
                           status = t.STATUS,
                           papers = t.PAPERS
                            
                   }); 

            return result;
        }

        public IEnumerable<RollViewModel> GenerateCustomersByridRegnoOrder(string coursecode, string namefilter)
        {
            var ccourse = CollegeRepository.GetLoginCollege();
            var result = new Entities().ROLLs.Where(x => x.COURSECODE == coursecode && x.COLLEGECODE == ccourse && string.IsNullOrEmpty(x.RSTATUS) &&  !string.IsNullOrEmpty(x.REGNO)).ToList().Select(t => new RollViewModel
            {
                coursecode = t.COURSECODE,
                rollno = t.ROLLNO,
                regno = t.REGNO,
                id = t.ID,
                mobileno = t.MOBILENO,
                name = t.NAME,
                status = t.STATUS,
                papers = t.PAPERS,
                 regu = t.REGU
                 

            }).OrderBy(x=> x.regno);

            return result;
        }
        public void Create(RollViewModel roll)
        {
            using (Entities nip = new Entities())
            {
                roll.id = Guid.NewGuid().ToString();
                nip.ROLLs.Add(new ROLL
                {
                    ID = roll.id,
                    COLLEGECODE = HttpContext.Current.User.Identity.Name.Substring(0,3),
                    COURSECODE = roll.coursecode,
                    ROLLNO = roll.rollno,
                    NAME = roll.name.ToUpper(),
                    REGU= "B"


                });
                nip.SaveChanges();
            }


        }
        public void Update(RollViewModel roll)
        {
            using (Entities nip = new Entities())
            { 
                var entity = nip.ROLLs.Where(x => x.ID.Equals(roll.id)).FirstOrDefault();

                if (entity != null)
                {
                    entity.COURSECODE = roll.coursecode;
                    entity.NAME = roll.name;
                    entity.ROLLNO = roll.rollno;
                    entity.MOBILENO = roll.mobileno;


                    nip.SaveChanges();
                } 
            }
        }
        public void Destroy(RollViewModel roll)
        {
            using (Entities nip = new Entities())
            {
                var delresult = nip.ROLLs.Where(x => x.ID.Equals(roll.id)).FirstOrDefault();
                if (delresult != null)
                {
                    nip.ROLLs.Remove(delresult);
                    nip.SaveChanges();
                }
            }
        }


    }
}