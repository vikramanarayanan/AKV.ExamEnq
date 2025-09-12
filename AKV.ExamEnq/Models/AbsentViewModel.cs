using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AKV.ExamEnq.Models
{
    public class AbsentViewModel
    {
        public string id { get; set; }
        public string collegecode { get; set; }
     //   [Required(ErrorMessage = "Select Course")]
        public string coursecode { get; set; }
        public string rollno { get; set; }
        public string regno { get; set; }
        public string name { get; set; }
        public string sub_no { get; set; }
    //    [Required(ErrorMessage = "Select Paper")]
        public string pap_no { get; set; }
        public string papname { get; set; }

        public bool status { get; set; }





    }
}