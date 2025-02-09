using System;
using System.ComponentModel.DataAnnotations;

namespace EHRSystem.Core.Models
{
    // [REQ: US-APT-01] Model for managing appointment time slots
    public class AppointmentSlot
    {
        public int Id { get; set; }

        [Required]
        public string DoctorId { get; set; }
        public ApplicationUser Doctor { get; set; }

        // [REQ: US-APT-01.1] Time slot details
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; } = true;

        // [REQ: US-APT-01.2] Buffer time management
        public bool HasBufferTime { get; set; }
        public DateTime? BufferStartTime { get; set; }
        public DateTime? BufferEndTime { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedById { get; set; }
        public ApplicationUser CreatedBy { get; set; }
    }
} 