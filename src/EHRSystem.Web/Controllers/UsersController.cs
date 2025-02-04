using EHRSystem.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EHRSystem.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            
            // Debug output
            Console.WriteLine("=== USER IDS ===");
            foreach (var user in users)
            {
                Console.WriteLine($"- {user.Id} | {user.Email}");
            }
            
            return View(users);
        }
    }
} 