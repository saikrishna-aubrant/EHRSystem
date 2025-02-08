using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EHRSystem.Core.Validation;

namespace EHRSystem.Core.ViewModels
{
    // US-PAT-04: Patient Dashboard View Model
    public class PatientDashboardViewModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string MRN { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        
        public List<AppointmentViewModel> UpcomingAppointments { get; set; } = new();
        public List<PatientVisitViewModel> RecentVisits { get; set; } = new();
        public List<TestResultViewModel> TestResults { get; set; } = new();
    }

    public class AppointmentViewModel
    {
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Purpose { get; set; }
        public string DoctorName { get; set; }
        public string Status { get; set; }
    }

    public class TestResultViewModel
    {
        public int Id { get; set; }
        public string TestName { get; set; }
        public DateTime TestDate { get; set; }
        public string Result { get; set; }
        public string NormalRange { get; set; }
        public string Status { get; set; }
        public string OrderedBy { get; set; }
    }

    public class AppointmentRequestViewModel
    {
        [Required]
        public string DoctorId { get; set; }
        
        [Required]
        [FutureDate]
        public DateTime PreferredDate { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Purpose { get; set; }
    }

    public class UpdateContactInfoViewModel
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
        
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        
        [RegularExpression(@"^\d{5}(-\d{4})?$")]
        public string? ZipCode { get; set; }
        
        [Required]
        public string EmergencyContactName { get; set; }
        
        [Required]
        [Phone]
        public string EmergencyContactPhone { get; set; }
        
        [Required]
        public string EmergencyContactRelation { get; set; }
    }
} 