using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using EHRSystem.Core.Models;
using EHRSystem.Core.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EHRSystem.Web.Controllers
{
    /// <summary>
    /// US-AUTH-01: Role Management - Create new system role
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RolesController(
            RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        /// <summary>
        /// US-AUTH-01: Role list view
        /// </summary>
        public IActionResult Index()
        {
            var roles = _roleManager.Roles;
            return View(roles);
        }

        // GET: Roles/Assign
        public async Task<IActionResult> Assign(string userId)
        {
            try 
            {
                if (string.IsNullOrEmpty(userId))
                    throw new ArgumentException("User ID is required");

                var user = await _userManager.FindByIdAsync(userId);
                
                if (user == null)
                {
                    var allUserIds = _userManager.Users.Select(u => u.Id).ToList();
                    throw new Exception($"User {userId} not found. Existing IDs: {string.Join(", ", allUserIds)}");
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var allRoles = _roleManager.Roles.ToList();
                
                var model = new AssignRolesViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Roles = allRoles.Select(r => new SelectListItem
                    {
                        Text = r.Name,
                        Value = r.Name,
                        Selected = userRoles.Contains(r.Name)
                    })
                };
                
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Assign(AssignRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            var currentRoles = await _userManager.GetRolesAsync(user);
            
            // Remove existing roles
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            
            // Add selected roles
            await _userManager.AddToRolesAsync(user, model.SelectedRoles);
            
            return RedirectToAction("Index", "Users");
        }

        /// <summary>
        /// US-AUTH-01: Create role form submission
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ApplicationRole role)
        {
            if (ModelState.IsValid)
            {
                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                    return RedirectToAction("Index");
            }
            return View(role);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        public async Task<IActionResult> EditUsers(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                return NotFound();
            }
            var users = await _userManager.GetUsersInRoleAsync(roleName);
            return View(users);
        }
    }
} 