using System.ComponentModel.DataAnnotations;

namespace EHRSystem.Core.Models
{
    public class MedicalHistory
    {
        public int Id { get; set; }
        
        [Required]
        public string PatientId { get; set; }
        public ApplicationUser Patient { get; set; }
        
        [Required]
        public string Condition { get; set; }
        
        public string? Description { get; set; }
        public DateTime DiagnosedDate { get; set; }
        public string? Treatment { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Audit fields
        public string CreatedById { get; set; }
        public ApplicationUser CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? LastModifiedById { get; set; }
        public ApplicationUser LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }

    public class Medication
    {
        public int Id { get; set; }
        
        [Required]
        public string PatientId { get; set; }
        public ApplicationUser Patient { get; set; }
        
        [Required]
        public string Name { get; set; }
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public DateTime PrescribedDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Audit fields
        public string PrescribedById { get; set; }
        public ApplicationUser PrescribedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? LastModifiedById { get; set; }
        public ApplicationUser LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }

    public class Allergy
    {
        public int Id { get; set; }
        
        [Required]
        public string PatientId { get; set; }
        public ApplicationUser Patient { get; set; }
        
        [Required]
        public string AllergenName { get; set; }
        public string? Reaction { get; set; }
        public string? Severity { get; set; }
        
        // Audit fields
        public string CreatedById { get; set; }
        public ApplicationUser CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? LastModifiedById { get; set; }
        public ApplicationUser LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }

    public class PatientVisit
    {
        public int Id { get; set; }
        
        [Required]
        public string PatientId { get; set; }
        public ApplicationUser Patient { get; set; }
        
        public DateTime VisitDate { get; set; }
        public string? Reason { get; set; }
        public string? Diagnosis { get; set; }
        public string? Treatment { get; set; }
        public string? Notes { get; set; }
        
        // Audit fields
        public string DoctorId { get; set; }
        public ApplicationUser Doctor { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? LastModifiedById { get; set; }
        public ApplicationUser LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }
} 