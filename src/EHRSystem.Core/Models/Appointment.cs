using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace EHRSystem.Core.Models
{
    // [REQ: US-APT-01] Appointment Model - Core entity for managing appointment scheduling
    public class Appointment
    {
        public int Id { get; set; }
        
        // [REQ: US-APT-01.1] Patient and Doctor Assignment
        [Required]
        public string PatientId { get; set; }
        public virtual ApplicationUser Patient { get; set; }
        
        [Required]
        public string DoctorId { get; set; }
        public virtual ApplicationUser Doctor { get; set; }
        
        // [REQ: US-APT-01.2] Appointment Time Management
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Purpose { get; set; }
        
        // [REQ: US-APT-01.3] Appointment Status Tracking
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // [REQ: US-APT-03.1] Change Tracking for Rescheduling
        public string? LastModifiedById { get; set; }
        public virtual ApplicationUser LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }

        // [REQ: US-APT-03.2] Appointment History for Audit
        public virtual ICollection<AppointmentAudit> AuditTrail { get; set; } = new List<AppointmentAudit>();
    }
} 