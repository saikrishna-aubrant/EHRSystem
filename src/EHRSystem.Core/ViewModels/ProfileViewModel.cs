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
        
        public string Role { get; set; }
    }
} 