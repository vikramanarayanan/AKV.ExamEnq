using AKV.ExamEnq.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Affiliated.Models
{
    public class RollModel
    {
        public string coursecode { get; set; }
        public string rollno { get; set; }
        public string name { get; set; }
        public string sex { get; set; }
        public string medium { get; set; }
        public string community { get; set; }
        public string mobileno { get; set; }
        public string image { get; set; }

        public List<SubmstViewModel> papers { get; set; }

        public string paidornot { get; set; }
        public decimal? examfee { get; set; }
    }
}