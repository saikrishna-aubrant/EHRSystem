using Microsoft.AspNetCore.Identity;  // Add this namespace
using System.ComponentModel.DataAnnotations;  // For [PersonalData]

namespace EHRSystem.Core.Models
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string FirstName { get; set; }

        [PersonalData]
        public string LastName { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class ApplicationRole : IdentityRole
{
    public ApplicationRole() { }  // Add parameterless constructor
    
    public ApplicationRole(string roleName) : base(roleName) { }  // Add this constructor
    
    public string Description { get; set; }
}
}