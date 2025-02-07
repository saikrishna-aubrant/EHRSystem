using System.ComponentModel.DataAnnotations;

namespace EHRSystem.Core.ViewModels
{
    public class ProfileViewModel
    {
        public string Id { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }
        
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        
        [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Invalid ZIP code format")]
        public string ZipCode { get; set; }
        
        public string Role { get; set; }
    }
} 