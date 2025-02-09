using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EHRSystem.Core.Models;
using EHRSystem.Core.ViewModels;
using EHRSystem.Data;
using EHRSystem.Data.Services;

namespace EHRSystem.Web.Controllers
{
    // [REQ: US-APT-01] Controller for appointment management
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly AppointmentService _appointmentService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EhrDbContext _context;

        public AppointmentsController(
            AppointmentService appointmentService,
            UserManager<ApplicationUser> userManager,
            EhrDbContext context)
        {
            _appointmentService = appointmentService;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            var userRoles = await _userManager.GetRolesAsync(currentUser);
            var role = userRoles.FirstOrDefault();

            var appointmentsQuery = role switch
            {
                "Admin" => _context.Appointments,
                "Doctor" => _context.Appointments.Where(a => a.DoctorId == currentUser.Id),
                "Patient" => _context.Appointments.Where(a => a.PatientId == currentUser.Id),
                _ => _context.Appointments.Where(a => 1 == 0) // Empty set
            };

            var appointments = await appointmentsQuery
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new AppointmentViewModel
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    Purpose = a.Purpose,
                    DoctorName = $"Dr. {a.Doctor.FirstName} {a.Doctor.LastName}",
                    PatientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
                    Status = a.Status
                })
                .ToListAsync();

            if (User.IsInRole("Patient"))
            {
                var doctors = await _userManager.GetUsersInRoleAsync("Doctor");
                ViewBag.Doctors = doctors.Select(d => new { Id = d.Id, Name = $"Dr. {d.FirstName} {d.LastName}" });
            }

            return View(appointments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmAppointment(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return NotFound();

            if (!User.IsInRole("Doctor") && !User.IsInRole("Admin"))
                return Forbid();

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && (a.DoctorId == currentUser.Id || User.IsInRole("Admin")));

            if (appointment == null)
                return NotFound();

            var success = await _appointmentService.ConfirmAppointment(id, currentUser.Id);
            if (!success)
                return BadRequest("Failed to confirm appointment");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id, string reason)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return NotFound();

            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
            if (appointment == null)
                return NotFound();

            // Check if user has permission to cancel this appointment
            if (!User.IsInRole("Admin") && 
                appointment.DoctorId != currentUser.Id && 
                appointment.PatientId != currentUser.Id)
                return Forbid();

            var success = await _appointmentService.CancelAppointment(id, currentUser.Id, reason);
            if (!success)
                return BadRequest("Failed to cancel appointment");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RescheduleAppointment(int id, DateTime newDateTime, string reason)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return NotFound();

            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
            if (appointment == null)
                return NotFound();

            // Only allow patients to reschedule their own appointments
            if (!User.IsInRole("Admin") && appointment.PatientId != currentUser.Id)
                return Forbid();

            var success = await _appointmentService.RescheduleAppointment(id, newDateTime, currentUser.Id, reason);
            if (!success)
                return BadRequest("Failed to reschedule appointment");

            return RedirectToAction(nameof(Index));
        }

        // [REQ: US-APT-01.14] Schedule appointment form
        [HttpGet]
        public async Task<IActionResult> Schedule(string doctorId = null)
        {
            if (!User.IsInRole("Patient"))
                return Forbid();

            var doctors = await _userManager.GetUsersInRoleAsync("Doctor");
            var currentUser = await _userManager.GetUserAsync(User);

            var viewModel = new AppointmentScheduleViewModel
            {
                DoctorList = doctors.Select(d => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = d.Id,
                    Text = $"Dr. {d.FirstName} {d.LastName}"
                }).ToList(),
                DoctorId = doctorId,
                AppointmentDate = DateTime.Today.AddDays(1),
                PatientId = currentUser.Id,
                PatientName = $"{currentUser.FirstName} {currentUser.LastName}"
            };

            return View(viewModel);
        }

        // [REQ: US-APT-01.15] Get available time slots
        [HttpGet]
        public async Task<IActionResult> GetTimeSlots(string doctorId, DateTime date)
        {
            if (!User.Identity.IsAuthenticated)
                return Forbid();

            var slots = await _appointmentService.GetAvailableSlots(doctorId, date);
            var timeSlots = slots.Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"{s.StartTime:HH:mm} - {s.EndTime:HH:mm}"
            });

            return Json(timeSlots);
        }

        // [REQ: US-APT-01.16] Create appointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Schedule(AppointmentScheduleViewModel model)
        {
            if (!User.IsInRole("Patient"))
                return Forbid();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var slot = await _context.AppointmentSlots.FindAsync(int.Parse(model.TimeSlotId));
                if (slot == null || !slot.IsAvailable)
                {
                    ModelState.AddModelError("", "Selected time slot is no longer available");
                    return View(model);
                }

                var appointment = await _appointmentService.CreateAppointment(
                    model.DoctorId,
                    model.PatientId,
                    slot.StartTime,
                    slot.EndTime,
                    model.ReasonForVisit
                );

                // [REQ: US-APT-01.17] Show confirmation
                var doctor = await _userManager.FindByIdAsync(model.DoctorId);
                var confirmationViewModel = new AppointmentConfirmationViewModel
                {
                    ReferenceNumber = _appointmentService.GenerateReferenceNumber(),
                    DoctorName = $"Dr. {doctor.FirstName} {doctor.LastName}",
                    AppointmentDate = slot.StartTime.Date,
                    TimeSlot = $"{slot.StartTime:HH:mm} - {slot.EndTime:HH:mm}",
                    PatientName = model.PatientName,
                    ReasonForVisit = model.ReasonForVisit
                };

                return View("Confirmation", confirmationViewModel);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }
    }
} 