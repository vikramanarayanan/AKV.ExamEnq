using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AKV.ExamEnq.Report
{
  public class RollPoco
    {
        public string collegecode { get; set; }
        public string collegename { get; set; }
        public string coursecode { get; set; }
        public string coursename { get; set; }
        public string regno { get; set; }
        public string rollno { get; set; }
        public string sem_no { get; set; }
       
        public string name { get; set; }
        public string sex { get; set; }
        public string medium { get; set; }
        public string community { get; set; }
        public string mobileno { get; set; }
         
        public string papers { get; set; }
        public string status { get; set; }
        public string rstatus { get; set; }
        public decimal examfee { get; set; }
        public decimal due { get; set; }
        public string paidornot { get; set; }
        public string regu { get; set; }
        public byte[] photo { get; set; }

        public byte[] qrcode { get; set; }

        //public List<RollPoco> test { get; set; }
    }
}
