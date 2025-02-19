﻿using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using ClinicManagement.Core;
using ClinicManagement.Core.Models;
using ClinicManagement.Core.ViewModel;

namespace ClinicManagement.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AppointmentsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ActionResult Index()
        {
            var appointments = _unitOfWork.Appointments.GetAppointments();
            return View(appointments);
        }

        public ActionResult Details(int id)
        {
            var appointment = _unitOfWork.Appointments.GetAppointmentWithPatient(id);
            return View("_AppointmentPartial", appointment);
        }
        //public ActionResult Patients(int id)
        //{
        //    var viewModel = new DoctorDetailViewModel()
        //    {
        //        Appointments = _unitOfWork.Appointments.GetAppointmentByDoctor(id),
        //    };
        //    //var upcomingAppnts = _unitOfWork.Appointments.GetAppointmentByDoctor(id);
        //    return View(viewModel);
        //}

        public ActionResult Create(int id)
        {
            var viewModel = new AppointmentFormViewModel
            {
                Patient = id,
                Doctors = _unitOfWork.Doctors.GetAvailableDoctors(),

                Heading = "New Appointment"
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AppointmentFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.Doctors = _unitOfWork.Doctors.GetAvailableDoctors();
                return View(viewModel);

            }
            var appointment = new Appointment()
            {
                StartDateTime = viewModel.GetStartDateTime(),
                Detail = viewModel.Detail,
                Status = false,
                PatientId = viewModel.Patient,
                Doctor = _unitOfWork.Doctors.GetDoctor(viewModel.Doctor)

            };
            //Check if the slot is available
            if (_unitOfWork.Appointments.ValidateAppointment(appointment.StartDateTime, viewModel.Doctor))
                return View("InvalidAppointment");

            _unitOfWork.Appointments.Add(appointment);
            _unitOfWork.Complete();

            var patient = _unitOfWork.Patients.GetPatient(viewModel.Patient);
            Email("Reminder-Your appointment on", viewModel.Date, patient.Name, appointment.Doctor.Name);
            return RedirectToAction("Index", "Appointments");
        }

        public static void Email(string htmlString, string date, string patientName, string doctorName)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress("parmarbhavin1012@gmail.com");
                    mail.To.Add("cg485@njit.edu");
                    mail.Subject = htmlString + date;
                    mail.Body = CreateEmailBody(patientName, doctorName, date);
                    mail.IsBodyHtml = true;
                  //  mail.Attachments.Add(new Attachment("C:\\file.zip"));

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential("parmarbhavin1012@gmail.com", "Vadodara@123");
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                };
            }
            catch (Exception ex) { }
        }

        public static string CreateEmailBody(string patientName, string doctorName, string dateTime)
        {
            string body = "Dear "+ patientName + "," + "\n" + "This is a friendly reminder confirming your appointment with" + doctorName + "on "+ dateTime + ".Please try to arrive 15 minutes early and bring your[IMPORTANT - DOCUMENT]." + "\n\n" + "If you have any questions or you need to reschedule, please call our office at (732)318-1413. Otherwise, we look forward to seeing you on "+ dateTime + ".Have a wonderful day! " + "\n" + "Warm regards," + "\n" + "CS 673";
            return body;
        }
        public ActionResult Edit(int id)
        {
            var appointment = _unitOfWork.Appointments.GetAppointment(id);
            var viewModel = new AppointmentFormViewModel()
            {
                Heading = "New Appointment",
                Id = appointment.Id,
                Date = appointment.StartDateTime.ToString("dd/MM/yyyy"),
                Time = appointment.StartDateTime.ToString("HH:mm"),
                Detail = appointment.Detail,
                Status = appointment.Status,
                Patient = appointment.PatientId,
                Doctor = appointment.DoctorId,
                //Patients = _unitOfWork.Patients.GetPatients(),
                Doctors = _unitOfWork.Doctors.GetDectors()
            };
            return View(viewModel);
        }

        public ActionResult Remove(int id)
        {
            var appointment = _unitOfWork.Appointments.GetAppointment(id);

            _unitOfWork.Appointments.Remove(appointment);
            EmailDelete("Deleted Appointment");
            return RedirectToAction("Index", "Appointments");
        }

        public static void EmailDelete(string htmlString)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress("parmarbhavin1012@gmail.com");
                    mail.To.Add("cg485@njit.edu");
                    mail.Subject = "Appointment Deleted";
                    mail.Body = "<h1>Appointment Booked</h1>";
                    mail.IsBodyHtml = true;
                    //  mail.Attachments.Add(new Attachment("C:\\file.zip"));

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential("parmarbhavin1012@gmail.com", "Vadodara@123");
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                };
            }
            catch (Exception ex) { }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AppointmentFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.Doctors = _unitOfWork.Doctors.GetDectors();
                viewModel.Patients = _unitOfWork.Patients.GetPatients();
                return View(viewModel);
            }

            var appointmentInDb = _unitOfWork.Appointments.GetAppointment(viewModel.Id);
            appointmentInDb.Id = viewModel.Id;
            appointmentInDb.StartDateTime = viewModel.GetStartDateTime();
            appointmentInDb.Detail = viewModel.Detail;
            appointmentInDb.Status = viewModel.Status;
            appointmentInDb.PatientId = viewModel.Patient;
            appointmentInDb.DoctorId = viewModel.Doctor;
            if (_unitOfWork.Appointments.ValidateAppointment(appointmentInDb.StartDateTime, viewModel.Doctor))
                return View("InvalidAppointment");

            _unitOfWork.Complete();
            EmailEdit("Appointment Edited");
            return RedirectToAction("Index");

        }

        public static void EmailEdit(string htmlString)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress("parmarbhavin1012@gmail.com");
                    mail.To.Add("cg485@njit.edu");
                    mail.Subject = "Appointment Edited";
                    mail.Body = "<h1>Appointment Booked</h1>";
                    mail.IsBodyHtml = true;
                    //  mail.Attachments.Add(new Attachment("C:\\file.zip"));

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential("parmarbhavin1012@gmail.com", "Vadodara@123");
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                };
            }
            catch (Exception ex) { }
        }
        public ActionResult DoctorsList()
        {
            var doctors = _unitOfWork.Doctors.GetAvailableDoctors();
            if (HttpContext.Request.IsAjaxRequest())
                return Json(new SelectList(doctors.ToArray(), "Id", "Name"), JsonRequestBehavior.AllowGet);
            return RedirectToAction("Create");
        }

        public PartialViewResult GetUpcommingAppointments(int id)
        {
            var appointments = _unitOfWork.Appointments.GetTodaysAppointments(id);
            return PartialView(appointments);
        }

    }
}