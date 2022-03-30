using EWA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;
using System.Net;
using System.Text.RegularExpressions;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using PagedList;
namespace EWA.Controllers
{
    public class HomeController : Controller
    {
        DBEntities db = new DBEntities();
        // GET: Home
        public ActionResult Index()
        {
       
            return View();
        }
        [HttpPost]
        public ActionResult Index(string qrText)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText,
            QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            return View(BitmapToBytes(qrCodeImage));
        }
        private static Byte[] BitmapToBytes(Bitmap img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        private SelectList GetDateList(int month = 0, int year = 0)
        {
            List<string> items = new List<string>();

            if (month == 0) month = DateTime.Today.Month;
            if (year == 0) year = DateTime.Today.Year;


            DateTime a = new DateTime(year, month, 1); // Start --> today
            DateTime b = a.AddMonths(+1).AddDays(-1); 
            // End ----> today + 6

            for (DateTime d = a; d <= b; d = d.AddDays(1))
            {
                items.Add(d.ToString("yyyy-MM-dd"));
            }

            return new SelectList(items);
        }


        // Generate and return time list
        private SelectList GetTimeList()
        {
            List<string> items = new List<string>();

            DateTime a = new DateTime(1, 1, 1, 9, 0, 0); // Start --> 09:00 (date not important)
            DateTime b = new DateTime(1, 1, 1, 18, 0, 0); // End ----> 18:00 (date not important)

            for (DateTime d = a; d <= b; d = d.AddMinutes(30))
            {
                items.Add(d.ToString("HH:mm"));
            }

            return new SelectList(items);
        }
        private SelectList GetMonthList()
        {
            // TODO: Return January (1) to Decemeber (12)
            var list = new List<object>();

            for (int i = 1; i <= 12; i++)
            {
                list.Add(new { Id = i, Name = new DateTime(1, i, 1).ToString("MMMM") });
            }

            return new SelectList(list, "Id", "Name");
        }

        private SelectList GetYearList(int min, int max, bool reverse = false)
        {
            // TODO: Return min to max years
            var list = new List<int>();

            for (int i = min; i <= max; i++)
            {
                list.Add(i);
            }

            if (reverse) list.Reverse();

            return new SelectList(list);
        }
        public ActionResult Appointment()
        {
           
            ViewBag.DoctorList = new SelectList(db.Doctors, "Username", "Name");
            ViewBag.DateList = GetDateList();
            ViewBag.TimeList = GetTimeList();

            return View();
        }

        [HttpPost]
        public ActionResult Appointment(AppointmentVM model)
        {
            if (db.Schedules.Find(model.Id) != null)
            {
                ModelState.AddModelError("Id", "Duplicated Id.");
            }
            if (ModelState.IsValidField("DateTime")) { 

             if (db.Schedules.Find(model.Date) != null && db.Schedules.Find(model.Time) !=null)
            {
               
                ModelState.AddModelError("DateTime", " DateTime Not Availables.");
            }
            }
        

                var patientmail = db.Patients.Where(m => m.Username == model.PatientId).FirstOrDefault(); 
            if (ModelState.IsValid)
            {
                // Database operation ignored
                var m = new Schedule
                {
                    Id = int.Parse(model.Id),
                    DoctorId = model.DoctorId,
                    DateTime = model.DateTime,
                    PatientId = patientmail.Username,
                    Remark = model.Remark
                };
                db.Schedules.Add(m);
                db.SaveChanges();
                SendConfirmationEmail(patientmail);

                TempData["info"] = $"Submitted: DoctorId = {model.DoctorId}, DateTime = {model.DateTime}";
             
                return RedirectToAction(null);
            }
         

            // Select list for doctor, date and time
            ViewBag.DoctorList = new SelectList(db.Doctors, "Username", "Name");
            ViewBag.DateList = GetDateList();
            ViewBag.TimeList = GetTimeList();

            return View(model);
        }

        private void SendConfirmationEmail(Patient patient)
        {
      
            MailMessage s = new MailMessage();
            s.To.Add($"{patient.Name} <{patient.Email}>");
            s.Subject = "Appointments Confirmation ";
            s.IsBodyHtml = true;
            s.Body = $@"
               <p>testing123</p>
            ";

            new SmtpClient().Send(s);
            TempData["Info"] = "Email sent.";

        }
      

        public ActionResult Doctor()
        {
            var model = db.Doctors;
            return View(model);
       
        }
        public ActionResult MAP()
        {
            return View();
        }
        public ActionResult Admin()
        {
            return View();
        }
        public ActionResult AppointmentHistory()
        {
            var model = db.Schedules;
            return View(model);
        }
        public ActionResult AppointmentHistoryDetail(int id)
        {
            var model = db.Schedules.Find(id);
            if (model == null)
            {
                return RedirectToAction("Index");
            }

            return View(model);
        }
        public ActionResult AppointmentEdit(int id)
        {
            var m = db.Schedules.Find(id);

            var model = new AppointmentVM
            {
                Id = m.Id.ToString(),
                DoctorId = m.DoctorId,
                //DateTime.Parse(Date + "T" + Time) = m.DateTime,
                PatientId = m.PatientId,
                Remark = m.Remark,
                Date = m.DateTime.ToString("yyyy-MM-dd"),
                Time = m.DateTime.ToString("HH:mm")
            };

            // Select list for doctor, date and time
            ViewBag.DoctorList = new SelectList(db.Doctors, "Username", "Name");
            ViewBag.DateList = GetDateList();
            ViewBag.TimeList = GetTimeList();

            return View(model);
        }
        [HttpPost]
        public ActionResult AppointmentEdit(AppointmentVM model)
        {
            var m = db.Schedules.Find(int.Parse(model.Id));

            if (m != null && ModelState.IsValid)
            {
                m.DoctorId = model.DoctorId;
                m.DateTime = model.DateTime;
                m.Remark = model.Remark;
                db.SaveChanges();
                TempData["Info"] = "Appointment edited.";

                return RedirectToAction(null);
            }
            

            // Select list for doctor, date and time
            ViewBag.DoctorList = new SelectList(db.Doctors, "Username", "Name");
            ViewBag.DateList = GetDateList();
            ViewBag.TimeList = GetTimeList();

            return View(model);
        }

        public ActionResult AppointmentCancel(int id)
        {
            var model = db.Schedules.Find(id);
            if (model != null)
            {
                db.Schedules.Remove(model);
                db.SaveChanges();
                TempData["Info"] = "Appointment Cancelled.";
            }

            return View(model);
        }
        public ActionResult Chat()
        {
            return View();
        }

        public ActionResult ScheduleShow(string date)
        {
            DateTime a = DateTime.Today;
            if (date != null)
            {
                a = DateTime.Parse(date);
            }
            DateTime b = a.AddDays(1);

       
            var model = db.Schedules.Where(e => e.DateTime >= a && e.DateTime < b).ToList();
            ViewBag.DateList = GetDateList();
       
            return View(model);
        }

        public ActionResult ScheduleList(string doctorId, string name = "", int page = 1)
        {
          
            ViewBag.DoctorList = db.Doctors;
            name = name.Trim();
            if (page < 1)
            {
                return RedirectToAction(null, new { page = 1 });
            }
            var model = db.Schedules.Where(e => e.Doctor.Username == doctorId || doctorId == null && e.Doctor.Name == name)
                .OrderBy(e=> e.Doctor.Username).ToPagedList(page,10);
          


            return View(model);
         
        }
        public ActionResult ScheduleSearch(string date, string time)
        {
            DateTime d = DateTime.Now;
            if (date != null && time != null)
            {
                d = DateTime.Parse(date + "T" + time);
            }

            var model = db.Schedules.Where(e => e.DateTime == d).ToList();
            ViewBag.DateList = GetDateList();
            ViewBag.TimeList = GetTimeList();
            return View(model);
        }


    }
}