using System;
using System.ComponentModel.DataAnnotations;

namespace EHRSystem.Core.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        
        [Required]
        public string PatientId { get; set; }
        public ApplicationUser Patient { get; set; }
        
        [Required]
        public string DoctorId { get; set; }
        public ApplicationUser Doctor { get; set; }
        
        public DateTime AppointmentDate { get; set; }
        public string Purpose { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public string? LastModifiedById { get; set; }
        public ApplicationUser LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }
} 