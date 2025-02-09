using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EHRSystem.Core.Models;

namespace EHRSystem.Data.Services
{
    // [REQ: US-APT-01] Service for managing appointments
    public class AppointmentService
    {
        private readonly EhrDbContext _context;
        private static readonly TimeSpan WorkingHoursStart = new TimeSpan(9, 0, 0); // 9 AM
        private static readonly TimeSpan WorkingHoursEnd = new TimeSpan(17, 0, 0); // 5 PM
        private static readonly TimeSpan SlotDuration = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan BufferTime = TimeSpan.FromMinutes(15);

        public AppointmentService(EhrDbContext context)
        {
            _context = context;
        }

        // [REQ: US-APT-01.8] Get available time slots
        public async Task<List<AppointmentSlot>> GetAvailableSlots(string doctorId, DateTime date)
        {
            var startOfDay = date.Date.Add(WorkingHoursStart);
            var endOfDay = date.Date.Add(WorkingHoursEnd);
            var slots = new List<AppointmentSlot>();

            // Get existing appointments for the doctor on this date
            var existingAppointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                           a.StartTime.Date == date.Date &&
                           a.Status != "Cancelled")
                .ToListAsync();

            // Generate all possible time slots
            var currentTime = startOfDay;
            while (currentTime.Add(SlotDuration) <= endOfDay)
            {
                var slotEnd = currentTime.Add(SlotDuration);
                var isAvailable = !existingAppointments.Any(a =>
                    (a.StartTime <= currentTime && a.EndTime > currentTime) ||
                    (a.StartTime < slotEnd && a.EndTime >= slotEnd));

                slots.Add(new AppointmentSlot
                {
                    DoctorId = doctorId,
                    StartTime = currentTime,
                    EndTime = slotEnd,
                    IsAvailable = isAvailable,
                    HasBufferTime = false
                });

                currentTime = currentTime.Add(SlotDuration);
            }

            return slots;
        }

        // [REQ: US-APT-01.9] Validate appointment time
        public bool ValidateAppointmentTime(DateTime startTime, DateTime endTime)
        {
            var duration = endTime - startTime;
            var timeOfDay = startTime.TimeOfDay;

            return timeOfDay >= WorkingHoursStart &&
                   endTime.TimeOfDay <= WorkingHoursEnd &&
                   duration >= SlotDuration &&
                   duration <= SlotDuration;
        }

        // [REQ: US-APT-01.10] Check for conflicts
        public async Task<bool> HasConflictingAppointments(string doctorId, DateTime startTime, DateTime endTime)
        {
            return await _context.Appointments
                .AnyAsync(a => a.DoctorId == doctorId &&
                              ((a.StartTime <= startTime && a.EndTime > startTime) ||
                               (a.StartTime < endTime && a.EndTime >= endTime)));
        }

        // [REQ: US-APT-01.11] Create appointment with buffer time
        public async Task<Appointment> CreateAppointment(string doctorId, string patientId, DateTime startTime, DateTime endTime, string purpose)
        {
            // Normalize the date to handle time zone differences
            var normalizedStartTime = startTime.Date.Add(startTime.TimeOfDay);
            var normalizedEndTime = normalizedStartTime.Add(SlotDuration);

            // Validate the time slot
            if (!await IsSlotAvailable(doctorId, normalizedStartTime))
            {
                throw new InvalidOperationException("Selected time slot is not available");
            }

            var appointment = new Appointment
            {
                DoctorId = doctorId,
                PatientId = patientId,
                StartTime = normalizedStartTime,
                EndTime = normalizedEndTime,
                AppointmentDate = normalizedStartTime.Date,
                Purpose = purpose,
                Status = "Requested",
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return appointment;
        }

        // [REQ: US-APT-01.12] Generate reference number
        public string GenerateReferenceNumber()
        {
            return $"APT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        // Add this method to the AppointmentService class
        public async Task<bool> IsSlotAvailable(string doctorId, DateTime startTime)
        {
            // Normalize the date to handle time zone differences
            var normalizedStartTime = startTime.Date.Add(startTime.TimeOfDay);
            var endTime = normalizedStartTime.Add(SlotDuration);
            
            // Check if the slot is within working hours
            var timeOfDay = normalizedStartTime.TimeOfDay;
            if (timeOfDay < WorkingHoursStart || timeOfDay.Add(SlotDuration) > WorkingHoursEnd)
            {
                return false;
            }

            // Check if there are any overlapping appointments
            var hasOverlap = await _context.Appointments
                .AnyAsync(a => a.DoctorId == doctorId &&
                              a.Status != "Cancelled" &&
                              a.StartTime.Date == normalizedStartTime.Date &&
                              ((a.StartTime <= normalizedStartTime && a.EndTime > normalizedStartTime) ||
                               (a.StartTime < endTime && a.EndTime >= endTime)));

            return !hasOverlap;
        }

        public async Task<bool> ConfirmAppointment(int appointmentId, string modifiedById)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null || appointment.Status != "Requested")
                return false;

            var oldStatus = appointment.Status;
            appointment.Status = "Confirmed";
            appointment.LastModifiedById = modifiedById;
            appointment.LastModifiedAt = DateTime.UtcNow;

            // Create audit trail
            var audit = new AppointmentAudit
            {
                AppointmentId = appointmentId,
                Action = "Confirm",
                Reason = "Doctor confirmed appointment",
                OldStatus = oldStatus,
                NewStatus = "Confirmed",
                ModifiedById = modifiedById,
                ModifiedAt = DateTime.UtcNow
            };
            _context.AppointmentAudits.Add(audit);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelAppointment(int appointmentId, string modifiedById, string reason)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
                return false;

            // Check if cancellation is allowed (24 hours before)
            if (appointment.AppointmentDate <= DateTime.UtcNow.AddHours(24))
                throw new InvalidOperationException("Appointments can only be cancelled at least 24 hours in advance.");

            // Update appointment status
            appointment.Status = "Cancelled";
            appointment.LastModifiedById = modifiedById;
            appointment.LastModifiedAt = DateTime.UtcNow;

            // Create audit trail
            var audit = new AppointmentAudit
            {
                AppointmentId = appointmentId,
                Action = "Cancel",
                Reason = reason,
                OldStatus = appointment.Status,
                NewStatus = "Cancelled",
                ModifiedById = modifiedById,
                ModifiedAt = DateTime.UtcNow
            };
            _context.AppointmentAudits.Add(audit);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RescheduleAppointment(int appointmentId, DateTime newTimeSlot, string modifiedById, string reason)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
                return false;

            // Check if rescheduling is allowed (24 hours before)
            if (appointment.AppointmentDate <= DateTime.UtcNow.AddHours(24))
                throw new InvalidOperationException("Appointments can only be rescheduled at least 24 hours in advance.");

            // Check if new time slot is available
            if (!await IsTimeSlotAvailable(appointment.DoctorId, newTimeSlot, appointmentId))
                throw new InvalidOperationException("Selected time slot is not available.");

            // Store old values for audit
            var oldDate = appointment.AppointmentDate;
            var oldStatus = appointment.Status;

            // Update appointment
            appointment.AppointmentDate = newTimeSlot;
            appointment.Status = "Requested"; // Reset to requested status for doctor to confirm
            appointment.LastModifiedById = modifiedById;
            appointment.LastModifiedAt = DateTime.UtcNow;

            // Create audit trail
            var audit = new AppointmentAudit
            {
                AppointmentId = appointmentId,
                Action = "Reschedule",
                Reason = reason,
                OldStatus = oldStatus,
                NewStatus = "Requested",
                OldDateTime = oldDate,
                NewDateTime = newTimeSlot,
                ModifiedById = modifiedById,
                ModifiedAt = DateTime.UtcNow
            };
            _context.AppointmentAudits.Add(audit);

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> IsTimeSlotAvailable(string doctorId, DateTime startTime, int? excludeAppointmentId = null)
        {
            var endTime = startTime.AddMinutes(30);
            var normalizedStartTime = startTime.ToUniversalTime();
            var normalizedEndTime = endTime.ToUniversalTime();

            // Check if there are any overlapping appointments
            var query = _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                           a.Status != "Cancelled" &&
                           a.AppointmentDate.Date == normalizedStartTime.Date);

            if (excludeAppointmentId.HasValue)
            {
                query = query.Where(a => a.Id != excludeAppointmentId.Value);
            }

            var hasOverlap = await query.AnyAsync(a =>
                (a.AppointmentDate <= normalizedStartTime && a.AppointmentDate.AddMinutes(30) > normalizedStartTime) ||
                (a.AppointmentDate < normalizedEndTime && a.AppointmentDate.AddMinutes(30) >= normalizedEndTime));

            return !hasOverlap;
        }
    }
} 