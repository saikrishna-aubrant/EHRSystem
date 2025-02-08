using System.ComponentModel.DataAnnotations;

namespace EHRSystem.Core.ViewModels
{
    public class PatientSearchViewModel
    {
        [Display(Name = "Patient ID")]
        public string? PatientId { get; set; }

        public string? Name { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Insurance Provider")]
        public string? InsuranceProvider { get; set; }

        [Display(Name = "Registration Date From")]
        [DataType(DataType.Date)]
        public DateTime? RegistrationDateFrom { get; set; }

        [Display(Name = "Registration Date To")]
        [DataType(DataType.Date)]
        public DateTime? RegistrationDateTo { get; set; }

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Sorting
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
    }

    public class PatientSearchResultViewModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? LastVisitDate { get; set; }
        public string InsuranceProvider { get; set; }
        public DateTime RegistrationDate { get; set; }

        // For pagination info
        public int TotalCount { get; set; }
        public int PageCount { get; set; }
        public int CurrentPage { get; set; }
    }
} 