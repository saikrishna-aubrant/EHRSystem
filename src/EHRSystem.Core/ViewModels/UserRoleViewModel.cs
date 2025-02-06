using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering; // Add this namespace

namespace EHRSystem.Core.ViewModels
{

public class UserRoleViewModel
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string CurrentRole { get; set; }
    public List<SelectListItem> AvailableRoles { get; set; }
}
}