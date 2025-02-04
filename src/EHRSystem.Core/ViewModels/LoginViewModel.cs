using System.ComponentModel.DataAnnotations;

namespace EHRSystem.Core.ViewModels
{
    // US-AUTH-02: Login credentials validation
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
} 