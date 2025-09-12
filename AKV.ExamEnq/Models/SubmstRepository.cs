using AKV.ExanEnq.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AKV.ExamEnq.Models
{
    public class SubmstRepository
    {
        public static SubmstRepository Repository = new SubmstRepository();
        public IEnumerable<SubmstViewModel> GetSubmst(string coursecode,string regu,int semno)
        {
            if(regu == "RJ")
            {
                var result = new Entities().SUBMSTs.Where(x => x.COURSECODE == coursecode).ToList().Select(t => new SubmstViewModel
                {
                    coursecode = t.COURSECODE,
                    papname1 = (string.IsNullOrEmpty(t.OPT)) ? t.PAP_NO + "->" + t.PAPNAME1 + "ㅤㅤ" : t.PAP_NO + "-" + t.OPT + "->" + t.PAPNAME1 + "ㅤㅤ",
                    pap_no = (string.IsNullOrEmpty(t.OPT)) ? t.PAP_NO : t.PAP_NO + "-" + t.OPT,
                    sem_no = t.SEM_NO.ToString(),
                    sub_no = t.SUB_NO,
                    opt = t.OPT,
                    regu = t.REGU

                }).OrderBy(x => x.sub_no).ThenBy(x => x.opt);

                return result;
            }
            else
            {
                var result = new Entities().SUBMSTs.Where(x => x.COURSECODE == coursecode && x.REGU == regu && x.SEM_NO <= semno).ToList().Select(t => new SubmstViewModel
                {
                    coursecode = t.COURSECODE,
                    papname1 = (string.IsNullOrEmpty(t.OPT)) ? t.PAP_NO + "->" + t.PAPNAME1 + "ㅤㅤ" : t.PAP_NO + "-" + t.OPT + "->" + t.PAPNAME1 + "ㅤㅤ",
                    pap_no = (string.IsNullOrEmpty(t.OPT)) ? t.PAP_NO : t.PAP_NO + "-" + t.OPT,
                    sem_no = t.SEM_NO.ToString(),
                    sub_no = t.SUB_NO,
                    opt = t.OPT,
                    regu = t.REGU

                }).OrderBy(x => x.sub_no).ThenBy(x => x.opt);

                return result;
            }

           
        }

    }
}