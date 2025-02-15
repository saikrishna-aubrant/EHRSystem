using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using EHRSystem.Core.Models;
using EHRSystem.Data;
using EHRSystem.Data.Services;
using Microsoft.AspNetCore.Antiforgery;

namespace EHRSystem.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppointmentService _appointmentService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EhrDbContext _context;
        private readonly IAntiforgery _antiforgery;

        public AppointmentsController(
            AppointmentService appointmentService,
            UserManager<ApplicationUser> userManager,
            EhrDbContext context,
            IAntiforgery antiforgery)
        {
            _appointmentService = appointmentService;
            _userManager = userManager;
            _context = context;
            _antiforgery = antiforgery;
        }

        [HttpGet("timeslots")]
        public async Task<IActionResult> GetTimeSlots([FromQuery] string doctorId, [FromQuery] DateTime date)
        {
            try
            {
                if (string.IsNullOrEmpty(doctorId))
                    return BadRequest("Doctor ID is required");

                if (date == default)
                    return BadRequest("Valid date is required");

                // Verify doctor exists
                var doctor = await _userManager.FindByIdAsync(doctorId);
                if (doctor == null)
                    return BadRequest("Doctor not found");

                var slots = await _appointmentService.GetAvailableSlots(doctorId, date);
                
                if (slots == null || !slots.Any())
                    return Ok(new { message = "No available time slots found for the selected date" });

                var availableSlots = slots.Where(s => s.IsAvailable)
                    .Select(s => new
                    {
                        id = s.Id,
                        startTime = s.StartTime,
                        endTime = s.EndTime,
                        text = $"{s.StartTime:HH:mm} - {s.EndTime:HH:mm}"
                    })
                    .ToList();

                return Ok(availableSlots);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error loading time slots: {ex.Message}");
            }
        }

        [HttpPost("request")]
        public async Task<IActionResult> RequestAppointment([FromBody] AppointmentRequestDto request)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return NotFound("User not found");

                // Get the slot to check its date
                var slot = await _appointmentService.GetSlotById(request.TimeSlotId);
                if (slot == null)
                    return BadRequest("Invalid time slot");

                if (!await _appointmentService.IsSlotAvailable(request.TimeSlotId, request.DoctorId, slot.StartTime.Date))
                    return BadRequest("Selected time slot is not available");

                var appointment = await _appointmentService.CreateAppointment(
                    request.DoctorId,
                    currentUser.Id,
                    request.TimeSlotId,
                    request.Purpose
                );

                return Ok(new { id = appointment.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/confirm")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> ConfirmAppointment(int id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return NotFound("User not found");

                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null)
                    return NotFound("Appointment not found");

                if (appointment.Status != "Requested")
                    return BadRequest("Only requested appointments can be confirmed");

                // For doctors, only allow confirming their own appointments
                if (User.IsInRole("Doctor") && appointment.DoctorId != currentUser.Id)
                    return Forbid();

                var success = await _appointmentService.ConfirmAppointment(id, currentUser.Id);
                if (!success)
                    return BadRequest("Failed to confirm appointment");

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("reschedule")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RescheduleAppointment([FromBody] AppointmentRescheduleDto request)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                var appointment = await _context.Appointments.FindAsync(request.AppointmentId);
                if (appointment == null)
                    return NotFound();

                // Check if user has permission to reschedule
                if (!User.IsInRole("Admin") && 
                    appointment.PatientId != currentUser.Id && 
                    appointment.DoctorId != currentUser.Id)
                {
                    return Forbid();
                }

                var success = await _appointmentService.RescheduleAppointment(
                    request.AppointmentId,
                    request.NewTimeSlot,
                    currentUser.Id,
                    request.Reason
                );

                if (!success)
                    return BadRequest("Failed to reschedule appointment");

                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id, [FromBody] AppointmentCancelDto request)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null)
                    return NotFound();

                // Check if user has permission to cancel
                if (!User.IsInRole("Admin") && 
                    appointment.PatientId != currentUser.Id && 
                    appointment.DoctorId != currentUser.Id)
                {
                    return Forbid();
                }

                var success = await _appointmentService.CancelAppointment(
                    id,
                    currentUser.Id,
                    request.Reason
                );

                if (!success)
                    return BadRequest("Failed to cancel appointment");

                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class AppointmentRequestDto
    {
        public string DoctorId { get; set; }
        public int TimeSlotId { get; set; }
        public string Purpose { get; set; }
    }

    public class AppointmentRescheduleDto
    {
        public int AppointmentId { get; set; }
        public DateTime NewTimeSlot { get; set; }
        public string Reason { get; set; }
    }

    public class AppointmentCancelDto
    {
        public string Reason { get; set; }
    }
} 