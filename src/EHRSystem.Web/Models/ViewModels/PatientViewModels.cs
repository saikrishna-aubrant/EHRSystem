using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EHRSystem.Web.Models.ViewModels
{
    public class PatientDashboardViewModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string MRN { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public List<AppointmentViewModel> UpcomingAppointments { get; set; }
        public List<PatientVisitViewModel> RecentVisits { get; set; }
        public List<TestResultViewModel> TestResults { get; set; }
    }

    public class AppointmentViewModel
    {
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Purpose { get; set; }
        public string DoctorName { get; set; }
        public string PatientName { get; set; }
        public string Status { get; set; }
    }

    public class PatientVisitViewModel
    {
        public int Id { get; set; }
        public DateTime VisitDate { get; set; }
        public string Reason { get; set; }
        public string Diagnosis { get; set; }
        public string DoctorName { get; set; }
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

    public class UpdateContactInfoViewModel
    {
        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Emergency Contact Name")]
        public string EmergencyContactName { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Emergency Contact Phone")]
        public string EmergencyContactPhone { get; set; }

        [Required]
        [Display(Name = "Relationship to Emergency Contact")]
        public string EmergencyContactRelation { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "State")]
        public string State { get; set; }

        [Display(Name = "ZIP Code")]
        [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Please enter a valid ZIP code.")]
        public string ZipCode { get; set; }
    }

    public class AppointmentRequestViewModel
    {
        [Required]
        [Display(Name = "Doctor")]
        public string DoctorId { get; set; }

        [Required]
        [Display(Name = "Preferred Date")]
        [DataType(DataType.Date)]
        [FutureDate(ErrorMessage = "Please select a future date.")]
        public DateTime PreferredDate { get; set; }

        [Required]
        [Display(Name = "Purpose of Visit")]
        public string Purpose { get; set; }
    }
}

public class FutureDateAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value is DateTime date)
        {
            return date.Date > DateTime.Today;
        }
        return false;
    }
} 