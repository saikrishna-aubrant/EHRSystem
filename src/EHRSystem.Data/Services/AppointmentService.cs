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
            try
            {
                // Ensure we're working with just the date part for comparison
                var targetDate = date.Date;

                // Get existing slots for the doctor on the selected date
                var slots = await _context.AppointmentSlots
                    .Where(s => s.DoctorId == doctorId 
                        && s.StartTime.Date == targetDate
                        && s.IsAvailable)
                    .OrderBy(s => s.StartTime)
                    .ToListAsync();

                // If no slots exist for this date, create them
                if (!slots.Any())
                {
                    var newSlots = GenerateTimeSlots(doctorId, targetDate);
                    await _context.AppointmentSlots.AddRangeAsync(newSlots);
                    await _context.SaveChangesAsync();
                    return newSlots;
                }

                return slots;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting available slots: {ex.Message}", ex);
            }
        }

        private List<AppointmentSlot> GenerateTimeSlots(string doctorId, DateTime date)
        {
            var slots = new List<AppointmentSlot>();
            var currentTime = date.Date.Add(WorkingHoursStart);
            var endTime = date.Date.Add(WorkingHoursEnd);

            while (currentTime.Add(SlotDuration) <= endTime)
            {
                var slot = new AppointmentSlot
                {
                    DoctorId = doctorId,
                    StartTime = currentTime,
                    EndTime = currentTime.Add(SlotDuration),
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = doctorId // Set the doctor as the creator of the slots
                };

                slots.Add(slot);
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
        public async Task<Appointment> CreateAppointment(string doctorId, string patientId, int slotId, string purpose)
        {
            var slot = await GetSlotById(slotId);
            if (slot == null || !slot.IsAvailable)
            {
                throw new InvalidOperationException("Selected time slot is not available.");
            }

            var appointment = new Appointment
            {
                DoctorId = doctorId,
                PatientId = patientId,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                AppointmentDate = slot.StartTime.Date,
                Purpose = purpose,
                Status = "Requested",
                CreatedAt = DateTime.UtcNow
            };

            slot.IsAvailable = false;

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return appointment;
        }

        // [REQ: US-APT-01.12] Generate reference number
        public string GenerateReferenceNumber()
        {
            return $"APT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        public async Task<bool> IsSlotAvailable(int slotId, string doctorId, DateTime appointmentDate)
        {
            return await _context.AppointmentSlots
                .AnyAsync(s => s.Id == slotId 
                    && s.DoctorId == doctorId 
                    && s.StartTime.Date == appointmentDate.Date 
                    && s.IsAvailable);
        }

        public async Task<AppointmentSlot> GetSlotById(int slotId)
        {
            return await _context.AppointmentSlots
                .FirstOrDefaultAsync(s => s.Id == slotId);
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

            // Only allow cancellation of Requested or Confirmed appointments
            if (appointment.Status != "Requested" && appointment.Status != "Confirmed")
                return false;

            var modifier = await _context.Users.FindAsync(modifiedById);
            if (modifier == null)
                return false;

            var userRoles = await _context.UserRoles
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new { ur.UserId, r.Name })
                .Where(ur => ur.UserId == modifiedById)
                .Select(ur => ur.Name)
                .ToListAsync();

            // Only enforce 24-hour restriction for patients
            if (!userRoles.Contains("Admin") && !userRoles.Contains("Doctor"))
            {
                if (appointment.AppointmentDate <= DateTime.Now.AddHours(24))
                {
                    throw new InvalidOperationException("Appointments can only be cancelled at least 24 hours in advance.");
                }
            }

            var oldStatus = appointment.Status;
            appointment.Status = "Cancelled";
            appointment.LastModifiedById = modifiedById;
            appointment.LastModifiedAt = DateTime.UtcNow;

            var audit = new AppointmentAudit
            {
                AppointmentId = appointmentId,
                Action = "Cancel",
                Reason = reason,
                OldStatus = oldStatus,
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
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Doctor)
                    .Include(a => a.Patient)
                    .FirstOrDefaultAsync(a => a.Id == appointmentId);

                if (appointment == null)
                    return false;

                // Only allow rescheduling of Requested or Confirmed appointments
                if (appointment.Status != "Requested" && appointment.Status != "Confirmed")
                    throw new InvalidOperationException("Only requested or confirmed appointments can be rescheduled.");

                var modifier = await _context.Users.FindAsync(modifiedById);
                if (modifier == null)
                    return false;

                var userRoles = await _context.UserRoles
                    .Join(_context.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => new { ur.UserId, r.Name })
                    .Where(ur => ur.UserId == modifiedById)
                    .Select(ur => ur.Name)
                    .ToListAsync();

                // Only enforce 24-hour restriction for patients
                if (!userRoles.Contains("Admin") && !userRoles.Contains("Doctor"))
                {
                    if (appointment.AppointmentDate <= DateTime.Now.AddHours(24))
                    {
                        throw new InvalidOperationException("Appointments can only be rescheduled at least 24 hours in advance.");
                    }
                }

                // Normalize the new time slot to ensure proper time handling
                var normalizedNewTimeSlot = DateTime.SpecifyKind(newTimeSlot, DateTimeKind.Utc);

                // Check if the new time slot is within working hours
                var timeOfDay = normalizedNewTimeSlot.TimeOfDay;
                if (timeOfDay < WorkingHoursStart || timeOfDay >= WorkingHoursEnd)
                {
                    throw new InvalidOperationException("Selected time is outside of working hours (9 AM - 5 PM).");
                }
                
                // Check if the new time slot is available
                if (!await IsTimeSlotAvailable(appointment.DoctorId, normalizedNewTimeSlot, appointmentId))
                {
                    throw new InvalidOperationException("Selected time slot is not available.");
                }

                // Store old values for audit
                var oldDate = appointment.AppointmentDate;
                var oldStatus = appointment.Status;

                // Update appointment times
                appointment.AppointmentDate = normalizedNewTimeSlot.Date;
                appointment.StartTime = normalizedNewTimeSlot;
                appointment.EndTime = normalizedNewTimeSlot.AddMinutes(30);
                appointment.Status = "Requested"; // Always set to Requested when rescheduling
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
                    NewDateTime = normalizedNewTimeSlot,
                    ModifiedById = modifiedById,
                    ModifiedAt = DateTime.UtcNow
                };

                _context.AppointmentAudits.Add(audit);

                // Mark the old time slot as available and the new one as unavailable
                var oldSlot = await _context.AppointmentSlots
                    .FirstOrDefaultAsync(s => s.DoctorId == appointment.DoctorId && 
                                            s.StartTime == oldDate);
                if (oldSlot != null)
                {
                    oldSlot.IsAvailable = true;
                }

                var newSlot = await _context.AppointmentSlots
                    .FirstOrDefaultAsync(s => s.DoctorId == appointment.DoctorId && 
                                            s.StartTime == normalizedNewTimeSlot);
                if (newSlot != null)
                {
                    newSlot.IsAvailable = false;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        private async Task<bool> IsTimeSlotAvailable(string doctorId, DateTime startTime, int? excludeAppointmentId = null)
        {
            var endTime = startTime.AddMinutes(30);
            
            // Ensure times are in UTC
            var normalizedStartTime = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
            var normalizedEndTime = DateTime.SpecifyKind(endTime, DateTimeKind.Utc);

            // Check if there are any overlapping appointments
            var query = _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                           a.Status != "Cancelled" &&
                           ((a.StartTime <= normalizedStartTime && a.EndTime > normalizedStartTime) ||
                            (a.StartTime < normalizedEndTime && a.EndTime >= normalizedEndTime)));

            if (excludeAppointmentId.HasValue)
            {
                query = query.Where(a => a.Id != excludeAppointmentId.Value);
            }

            var hasOverlap = await query.AnyAsync();

            // Also check if the time slot exists and is available
            var slot = await _context.AppointmentSlots
                .FirstOrDefaultAsync(s => s.DoctorId == doctorId && 
                                        s.StartTime == normalizedStartTime);

            // If no slot exists, create one
            if (slot == null)
            {
                return true; // Allow booking if no explicit slot exists
            }

            return !hasOverlap && slot.IsAvailable;
        }
    }
} 