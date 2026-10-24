using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AKV.ExamEnq.Report
{
    public class HallTicketPoco
    {
      

        public string demand { get; set; }
        public string paid { get; set; }
        public string venname { get; set; }
        public string coursecode { get; set; }
        public string collegecode { get; set; }
        public string collegename { get; set; }
     
        public string coursename { get; set; }
        public string regno { get; set; }
        public string regu { get; set; }
        public string rollno { get; set; }
        public string sem_no { get; set; }
        public string pap_no { get; set; }
        public string papname1 { get; set; }
        public string opt { get; set; }

        public string name { get; set; }
        
        public string medium { get; set; }
        public string community { get; set; }
        public string mobileno { get; set; }
 
        public string sub_no { get; set; }
        public string prac { get; set; }
        public string edate { get; set; }
        public string ses { get; set; }
       
        public byte[] photo { get; set; }

        public byte[] qrcode { get; set; }
    }
}
