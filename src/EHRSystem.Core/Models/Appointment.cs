using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace EHRSystem.Core.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        
        [Required]
        public string PatientId { get; set; }
        public virtual ApplicationUser Patient { get; set; }
        
        [Required]
        public string DoctorId { get; set; }
        public virtual ApplicationUser Doctor { get; set; }
        
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Purpose { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public string? LastModifiedById { get; set; }
        public virtual ApplicationUser LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }

        public virtual ICollection<AppointmentAudit> AuditTrail { get; set; } = new List<AppointmentAudit>();
    }
} 