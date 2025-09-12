using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AKV.ExamEnq.Models
{
    public class RollViewModel
    {
      
        public  string id { get; set; }
        public string collegecode { get; set; }
        [Required(ErrorMessage = "Enter Coursecode")]
        [Display(Name = "Coursecode")]
        public string coursecode { get; set; }

        public string pap_no { get; set; }
        public string community { get; set; }
        public string medium { get; set; }
        public string papers { get; set; }

        [Required(ErrorMessage = "Enter Name")]
       [Display(Name = "Name")]
       public string name { get; set; }
        [Required(ErrorMessage = "Enter Rollno")]
        [Display(Name = "Rollno")]
        public string rollno { get; set; }
        public string regno { get; set; }
        public string regu { get; set; }
        public string sex { get; set; }

        [Display(Name = "Mobile No")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "The {0} must be number")]
         
       public string mobileno { get; set; }
        public string status { get; set; }


    }
}