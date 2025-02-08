using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EHRSystem.Core.ViewModels;
using EHRSystem.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EHRSystem.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminPanel()
        {
            var users = _userManager.Users.ToList();
            var userRoles = new List<UserRoleViewModel>();
            
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    CurrentRole = roles.FirstOrDefault()
                });
            }
            
            return View(userRoles);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            var model = new RegisterViewModel
            {
                RoleOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Patient", Text = "Patient" }
                }
            };
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminRegister()
        {
            var model = new RegisterViewModel
            {
                RoleOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Doctor", Text = "Doctor" },
                    new SelectListItem { Value = "Patient", Text = "Patient" }
                }
            };
            return View("Register", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Remove role validation from ModelState if not admin
            if (!User.IsInRole("Admin"))
            {
                ModelState.Remove("Role");
                ModelState.Remove("RoleOptions");
            }

            if (ModelState.IsValid)
            {
                // Validate date of birth
                if (model.DateOfBirth >= DateTime.Today)
                {
                    ModelState.AddModelError("DateOfBirth", "Date of birth cannot be in the future");
                    return View(model);
                }

                string mrn = await GenerateMRN();
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    MiddleName = model.MiddleName,
                    LastName = model.LastName,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    City = model.City,
                    State = model.State,
                    ZipCode = model.ZipCode,
                    EmergencyContactName = model.EmergencyContactName,
                    EmergencyContactPhone = model.EmergencyContactPhone,
                    EmergencyContactRelation = model.EmergencyContactRelation,
                    InsuranceProvider = model.InsuranceProvider,
                    InsurancePolicyNumber = model.InsurancePolicyNumber,
                    MRN = mrn
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    string role = User.IsInRole("Admin") ? model.Role : "Patient";
                    await _userManager.AddToRoleAsync(user, role);

                    // Confirm email automatically
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _userManager.ConfirmEmailAsync(user, token);

                    // Success message
                    TempData["SuccessMessage"] = $"Welcome {user.FirstName}! Your account has been created successfully.";

                    if (!User.IsInRole("Admin"))
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                    }

                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        private async Task<string> GenerateMRN()
        {
            // Generate unique MRN (Medical Record Number)
            string prefix = "MRN";
            string timestamp = DateTime.Now.ToString("yyyyMMdd");
            string random = new Random().Next(1000, 9999).ToString();
            return $"{prefix}{timestamp}{random}";
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, newRole);
            }
            return RedirectToAction("AdminPanel");
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // US-AUTH-02: Login functionality
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    // Check if user is active
                    if (!user.IsActive)
                    {
                        ModelState.AddModelError(string.Empty, "Account is deactivated.");
                        return View(model);
                    }

                    var result = await _signInManager.PasswordSignInAsync(
                        model.Email, 
                        model.Password, 
                        model.RememberMe, 
                        lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        return LocalRedirect(returnUrl);
                    }
                    if (result.RequiresTwoFactor)
                    {
                        return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                    }
                    if (result.IsLockedOut)
                    {
                        return RedirectToPage("./Lockout");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // US-AUTH-03: Password reset flow
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    // Instead of sending email, just show a message
                    TempData["InfoMessage"] = "If your email is registered, you will receive password reset instructions shortly.";
                }
                return View("ForgotPasswordConfirmation");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return BadRequest("Invalid password reset token");
            }
            
            return View(new ResetPasswordViewModel { Email = email, Token = token });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            AddErrors(result);
            
            return View(model);
        }

        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            
            var model = new ProfileViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = roles.FirstOrDefault()
            };
            
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            user.Email = model.Email;
            user.UserName = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["StatusMessage"] = "Your profile has been updated";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
    }
} 