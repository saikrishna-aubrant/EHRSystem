using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EHRSystem.Core.ViewModels
{
    public class AssignRolesViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public IEnumerable<SelectListItem> Roles { get; set; }
        public List<string> SelectedRoles { get; set; } = new();
    }
}