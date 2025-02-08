using Microsoft.AspNetCore.Identity;  // Add this namespace
using System.ComponentModel.DataAnnotations;  // For [PersonalData]

namespace EHRSystem.Core.Models
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        [Required]
        public string FirstName { get; set; }

        [PersonalData]
        [Required]
        public string LastName { get; set; }

        public bool IsActive { get; set; } = true;

        // New patient-specific fields
        public string? MiddleName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        
        // Emergency Contact
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactRelation { get; set; }
        
        // Insurance Information (Optional)
        public string? InsuranceProvider { get; set; }
        public string? InsurancePolicyNumber { get; set; }
        
        // Medical Record Number
        public string? MRN { get; set; }

        // Registration and Visit Info
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastVisitDate { get; set; }

        // Audit fields
        public string? LastModifiedById { get; set; }
        public ApplicationUser? LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }

    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() { }

        public ApplicationRole(string roleName) : base(roleName) 
        {
            Description = $"{roleName} role"; // Set a default description
        }
        
        public string? Description { get; set; }
    }
}