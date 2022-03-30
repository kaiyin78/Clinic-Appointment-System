using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EWA.Models
{
    public class DoctorMetadata
    {
        public string Username { get; set; }
    }
    [MetadataType(typeof(DoctorMetadata))]

    public partial class Doctor { }
    public class ScheduleMetadata
    {
        public int Id { get; set; }
    }
    [MetadataType(typeof(ScheduleMetadata))]

    public partial class Schedule { }

    public class QRModel

    {
        public string Message { get; set; }

    }

}