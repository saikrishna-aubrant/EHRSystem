using System;
using System.ComponentModel.DataAnnotations;

namespace EHRSystem.Core.Models
{
    // [REQ: US-APT-01] AppointmentSlot Model - Manages doctor availability for scheduling
    public class AppointmentSlot
    {
        public int Id { get; set; }

        // [REQ: US-APT-01.1] Doctor Assignment and Availability
        [Required]
        public string DoctorId { get; set; }
        public ApplicationUser Doctor { get; set; }

        // [REQ: US-APT-01.2] Time Slot Configuration
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; } = true;

        // [REQ: US-APT-01.3] Buffer Time Requirements
        public bool HasBufferTime { get; set; }
        public DateTime? BufferStartTime { get; set; }
        public DateTime? BufferEndTime { get; set; }

        // [REQ: US-APT-01.4] Slot Creation Tracking
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedById { get; set; }
        public ApplicationUser CreatedBy { get; set; }
    }
} 