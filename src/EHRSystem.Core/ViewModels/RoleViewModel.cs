using System.ComponentModel.DataAnnotations;

namespace EHRSystem.Core.ViewModels
{
    public class RoleViewModel
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Description { get; set; }
    }
} 