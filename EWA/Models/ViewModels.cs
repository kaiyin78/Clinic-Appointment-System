using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EWA.Models
{
    public class AppointmentVM
    {
        [Required]
        [StringLength(20)]
        // [Remote("CheckId", "Home", ErrorMessage = "Duplicated {0}.")]
        public string Id { get; set; }
        [Required]
        public string DoctorId { get; set; }

        // Date as string first, later will convert to DateTime object
        [Required]
        // [System.Web.Mvc.Remote("CheckDate", "Home", ErrorMessage = "Duplicated {0}.")]
        public string Date { get; set; }

        // Time as string first, later will convert to DateTime object
        [Required]
        // [System.Web.Mvc.Remote("CheckTime", "Home", ErrorMessage = "Duplicated {0}.")]
        public string Time { get; set; }
        // Combine date and time together (as DateTime object)
        public DateTime DateTime
        {
            get
            {
                return DateTime.Parse(Date + "T" + Time);
            }
        
       }
        public string PatientId { get; set; }
        public string Remark { get; set; }

    }
}