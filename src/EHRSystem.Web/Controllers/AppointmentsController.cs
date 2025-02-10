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
    // [REQ: US-APT-01] Appointment Management Controller - Handles all appointment-related operations
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

        // [REQ: US-APT-02] Patient Appointment Scheduling - Allows patients to schedule new appointments
        [HttpGet]
        public async Task<IActionResult> Schedule()
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
                AppointmentDate = DateTime.Today.AddDays(1)
            };

            return View(viewModel);
        }

        // [REQ: US-APT-03] Time Slot Management - Retrieves available time slots for appointment scheduling
        [HttpGet]
        public async Task<IActionResult> GetTimeSlots(string doctorId, DateTime date)
        {
            if (string.IsNullOrEmpty(doctorId) || date == default)
                return BadRequest();

            try
            {
                // Get available slots using the service
                var slots = await _appointmentService.GetAvailableSlots(doctorId, date);

                // Map to response format
                var slotList = slots.Select(s => new
                {
                    id = s.Id,
                    text = $"{s.StartTime:HH:mm} - {s.EndTime:HH:mm}"
                }).ToList();

                return Json(slotList);
            }
            catch (Exception ex)
            {
                return BadRequest("Error loading time slots: " + ex.Message);
            }
        }

        // [REQ: US-APT-02.1] Appointment Creation - Processes new appointment requests
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Schedule(AppointmentScheduleViewModel model)
        {
            if (!User.IsInRole("Patient"))
                return Forbid();

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "User not found";
                    return RedirectToAction(nameof(Schedule));
                }

                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    TempData["ErrorMessage"] = $"Please fill in all required fields: {errors}";
                    return RedirectToAction(nameof(Schedule));
                }

                int slotId;
                if (!int.TryParse(model.TimeSlotId, out slotId))
                {
                    TempData["ErrorMessage"] = "Invalid time slot format";
                    return RedirectToAction(nameof(Schedule));
                }

                // Check if the slot is still available
                if (!await _appointmentService.IsSlotAvailable(slotId, model.DoctorId, model.AppointmentDate))
                {
                    TempData["ErrorMessage"] = "The selected time slot is no longer available. Please choose another slot.";
                    return RedirectToAction(nameof(Schedule));
                }

                // Create the appointment using the service
                var appointment = await _appointmentService.CreateAppointment(
                    model.DoctorId,
                    currentUser.Id,
                    slotId,
                    model.ReasonForVisit
                );

                // Get the doctor's information
                var doctor = await _userManager.FindByIdAsync(model.DoctorId);
                
                // Get the slot information
                var slot = await _appointmentService.GetSlotById(slotId);

                // Prepare confirmation view model
                var confirmationViewModel = new AppointmentConfirmationViewModel
                {
                    ReferenceNumber = appointment.Id.ToString("D6"),
                    DoctorName = $"Dr. {doctor.FirstName} {doctor.LastName}",
                    AppointmentDate = slot.StartTime.Date,
                    TimeSlot = $"{slot.StartTime:HH:mm} - {slot.EndTime:HH:mm}",
                    PatientName = $"{currentUser.FirstName} {currentUser.LastName}",
                    ReasonForVisit = model.ReasonForVisit
                };

                return View("Confirmation", confirmationViewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while scheduling the appointment: {ex.Message}";
                return RedirectToAction(nameof(Schedule));
            }
        }

        // [REQ: US-APT-04] Appointment List View - Shows appointments based on user role
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            var query = User.IsInRole("Patient") 
                ? _context.Appointments.Where(a => a.PatientId == currentUser.Id)
                : User.IsInRole("Doctor") 
                    ? _context.Appointments.Where(a => a.DoctorId == currentUser.Id)
                    : _context.Appointments;

            var appointments = await query
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

            return View(appointments);
        }

        // [REQ: US-APT-05] Appointment Cancellation - Handles appointment cancellation requests
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return NotFound();

            if (appointment.PatientId != currentUser.Id && appointment.DoctorId != currentUser.Id && !User.IsInRole("Admin"))
                return Forbid();

            // Find and free up the slot
            var slot = await _context.AppointmentSlots
                .FirstOrDefaultAsync(s => s.DoctorId == appointment.DoctorId 
                    && s.StartTime == appointment.StartTime);

            if (slot != null)
            {
                slot.IsAvailable = true;
            }

            appointment.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // [REQ: US-APT-06] Appointment Rescheduling - Handles appointment rescheduling requests
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RescheduleAppointment(int id, DateTime newDateTime, string reason)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return NotFound();

                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null)
                    return NotFound();

                // Check if user has permission to reschedule
                if (appointment.PatientId != currentUser.Id && 
                    appointment.DoctorId != currentUser.Id && 
                    !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Convert the newDateTime to UTC and normalize it
                var utcDateTime = DateTime.SpecifyKind(newDateTime, DateTimeKind.Utc);
                
                // Store old values for audit
                var oldDate = appointment.AppointmentDate;
                var oldStatus = appointment.Status;

                // Update appointment details
                appointment.AppointmentDate = utcDateTime;
                appointment.StartTime = utcDateTime;
                appointment.EndTime = utcDateTime.AddMinutes(30);
                appointment.Status = "Requested"; // Always set to Requested when rescheduling
                appointment.LastModifiedById = currentUser.Id;
                appointment.LastModifiedAt = DateTime.UtcNow;

                // Create audit entry
                var audit = new AppointmentAudit
                {
                    AppointmentId = appointment.Id,
                    Action = "Reschedule",
                    OldDateTime = oldDate,
                    NewDateTime = utcDateTime,
                    OldStatus = oldStatus,
                    NewStatus = "Requested",
                    Reason = reason,
                    ModifiedById = currentUser.Id,
                    ModifiedAt = DateTime.UtcNow
                };

                _context.AppointmentAudits.Add(audit);

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Appointment rescheduled successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "Failed to save changes to the database.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while rescheduling the appointment.";
                return RedirectToAction(nameof(Index));
            }
        }

        // [REQ: US-APT-07] Appointment Confirmation - Allows doctors to confirm appointment requests
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmAppointment(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return NotFound();

            if (!User.IsInRole("Doctor") && !User.IsInRole("Admin"))
                return Forbid();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            // Check if doctor has permission to confirm
            if (User.IsInRole("Doctor") && appointment.DoctorId != currentUser.Id)
                return Forbid();

            try
            {
                var success = await _appointmentService.ConfirmAppointment(id, currentUser.Id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Appointment confirmed successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to confirm appointment.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while confirming the appointment.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
} 