using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EHRSystem.Core.ViewModels;
using EHRSystem.Core.Models;
using EHRSystem.Data;
using EHRSystem.Data.Services;
using System.Linq.Dynamic.Core;
using System;
using System.Threading.Tasks;

namespace EHRSystem.Web.Controllers
{
    public class PatientController : Controller
    {
        private readonly EhrDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public PatientController(
            EhrDbContext context, 
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "Admin,Doctor,Nurse")]
        public IActionResult Search()
        {
            return View(new PatientSearchViewModel());
        }

        [Authorize(Roles = "Admin,Doctor,Nurse")]
        [HttpGet]
        public async Task<IActionResult> SearchResults([FromQuery] PatientSearchViewModel searchModel)
        {
            // Get users in Patient role
            var patientRoleId = (await _roleManager.FindByNameAsync("Patient"))?.Id;
            
            var query = from user in _userManager.Users
                       join userRole in _context.UserRoles on user.Id equals userRole.UserId
                       where userRole.RoleId == patientRoleId
                       select user;

            // Apply filters
            if (!string.IsNullOrEmpty(searchModel.PatientId))
            {
                query = query.Where(p => p.MRN.Contains(searchModel.PatientId));
            }

            if (!string.IsNullOrEmpty(searchModel.Name))
            {
                var searchTerm = searchModel.Name.ToLower();
                query = query.Where(p => 
                    p.FirstName.ToLower().Contains(searchTerm) || 
                    p.LastName.ToLower().Contains(searchTerm));
            }

            if (searchModel.DateOfBirth.HasValue)
            {
                query = query.Where(p => p.DateOfBirth.Date == searchModel.DateOfBirth.Value.Date);
            }

            if (!string.IsNullOrEmpty(searchModel.PhoneNumber))
            {
                query = query.Where(p => p.PhoneNumber.Contains(searchModel.PhoneNumber));
            }

            if (!string.IsNullOrEmpty(searchModel.InsuranceProvider))
            {
                query = query.Where(p => p.InsuranceProvider == searchModel.InsuranceProvider);
            }

            if (searchModel.RegistrationDateFrom.HasValue)
            {
                query = query.Where(p => p.RegistrationDate.Date >= searchModel.RegistrationDateFrom.Value.Date);
            }

            if (searchModel.RegistrationDateTo.HasValue)
            {
                query = query.Where(p => p.RegistrationDate.Date <= searchModel.RegistrationDateTo.Value.Date);
            }

            // Apply sorting
            var sortBy = string.IsNullOrEmpty(searchModel.SortBy) ? "LastName" : searchModel.SortBy;
            var sortDirection = string.IsNullOrEmpty(searchModel.SortDirection) ? "asc" : searchModel.SortDirection;
            
            query = query.OrderBy($"{sortBy} {sortDirection}");

            // Get total count for pagination
            var totalCount = await query.CountAsync();
            var pageCount = (int)Math.Ceiling(totalCount / (double)searchModel.PageSize);

            // Apply pagination
            var patients = await query
                .Skip((searchModel.PageNumber - 1) * searchModel.PageSize)
                .Take(searchModel.PageSize)
                .Select(p => new PatientSearchResultViewModel
                {
                    Id = p.Id,
                    FullName = $"{p.FirstName} {p.LastName}",
                    DateOfBirth = p.DateOfBirth,
                    PhoneNumber = p.PhoneNumber,
                    LastVisitDate = p.LastVisitDate,
                    InsuranceProvider = p.InsuranceProvider,
                    RegistrationDate = p.RegistrationDate,
                    TotalCount = totalCount,
                    PageCount = pageCount,
                    CurrentPage = searchModel.PageNumber
                })
                .ToListAsync();

            return PartialView("_SearchResults", patients);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(string id)
        {
            var patient = await _userManager.FindByIdAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            // Check if the user is authorized to view this patient's details
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            // Debug information
            Console.WriteLine($"Current User ID: {currentUser.Id}");
            Console.WriteLine($"Patient ID: {id}");
            Console.WriteLine($"Is Admin: {User.IsInRole("Admin")}");
            Console.WriteLine($"Is Doctor: {User.IsInRole("Doctor")}");
            Console.WriteLine($"Is Same User: {id == currentUser.Id}");

            // Allow access if:
            // 1. User is viewing their own record
            // 2. User is an Admin
            // 3. User is a Doctor
            // 4. User is a Nurse
            if (id != currentUser.Id && 
                !User.IsInRole("Admin") && 
                !User.IsInRole("Doctor") && 
                !User.IsInRole("Nurse"))
            {
                return Forbid();
            }

            var viewModel = new PatientProfileViewModel
            {
                Id = patient.Id,
                FullName = $"{patient.FirstName} {patient.LastName}",
                MRN = patient.MRN,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                Email = patient.Email,
                PhoneNumber = patient.PhoneNumber,
                Address = patient.Address,
                City = patient.City,
                State = patient.State,
                ZipCode = patient.ZipCode,
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactPhone = patient.EmergencyContactPhone,
                EmergencyContactRelation = patient.EmergencyContactRelation,
                InsuranceProvider = patient.InsuranceProvider,
                InsurancePolicyNumber = patient.InsurancePolicyNumber,
                RegistrationDate = patient.RegistrationDate
            };

            // Load medical information for Doctors, Admins, and the patient viewing their own record
            if (User.IsInRole("Doctor") || User.IsInRole("Admin") || id == currentUser.Id)
            {
                // Debug information
                Console.WriteLine("Loading medical information...");
                
                var medicalHistory = await _context.MedicalHistories
                    .Where(m => m.PatientId == id)
                    .OrderByDescending(m => m.DiagnosedDate)
                    .ToListAsync();

                Console.WriteLine($"Found {medicalHistory.Count} medical history records");

                viewModel.MedicalHistory = medicalHistory.Select(m => new MedicalHistoryViewModel
                {
                    Id = m.Id,
                    Condition = m.Condition,
                    Description = m.Description,
                    DiagnosedDate = m.DiagnosedDate,
                    Treatment = m.Treatment,
                    IsActive = m.IsActive,
                    CreatedBy = m.CreatedBy?.FirstName + " " + m.CreatedBy?.LastName,
                    CreatedAt = m.CreatedAt
                }).ToList();

                var medications = await _context.Medications
                    .Where(m => m.PatientId == id && m.IsActive)
                    .OrderByDescending(m => m.PrescribedDate)
                    .ToListAsync();

                Console.WriteLine($"Found {medications.Count} medications");

                viewModel.Medications = medications.Select(m => new MedicationViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    Dosage = m.Dosage,
                    Frequency = m.Frequency,
                    PrescribedDate = m.PrescribedDate,
                    EndDate = m.EndDate,
                    IsActive = m.IsActive,
                    PrescribedBy = m.PrescribedBy?.FirstName + " " + m.PrescribedBy?.LastName
                }).ToList();

                var allergies = await _context.Allergies
                    .Where(a => a.PatientId == id)
                    .ToListAsync();

                Console.WriteLine($"Found {allergies.Count} allergies");

                viewModel.Allergies = allergies.Select(a => new AllergyViewModel
                {
                    Id = a.Id,
                    AllergenName = a.AllergenName,
                    Reaction = a.Reaction,
                    Severity = a.Severity
                }).ToList();

                var visits = await _context.PatientVisits
                    .Where(v => v.PatientId == id)
                    .OrderByDescending(v => v.VisitDate)
                    .Take(5)
                    .ToListAsync();

                Console.WriteLine($"Found {visits.Count} visits");

                viewModel.RecentVisits = visits.Select(v => new PatientVisitViewModel
                {
                    Id = v.Id,
                    VisitDate = v.VisitDate,
                    Reason = v.Reason,
                    Diagnosis = v.Diagnosis,
                    Treatment = v.Treatment,
                    DoctorName = v.Doctor?.FirstName + " " + v.Doctor?.LastName
                }).ToList();
            }

            return View(viewModel);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Edit(string id)
        {
            var patient = await _userManager.FindByIdAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            var viewModel = new PatientProfileViewModel
            {
                Id = patient.Id,
                FullName = $"{patient.FirstName} {patient.LastName}",
                MRN = patient.MRN,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                Email = patient.Email,
                PhoneNumber = patient.PhoneNumber,
                Address = patient.Address,
                City = patient.City,
                State = patient.State,
                ZipCode = patient.ZipCode,
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactPhone = patient.EmergencyContactPhone,
                EmergencyContactRelation = patient.EmergencyContactRelation,
                InsuranceProvider = patient.InsuranceProvider,
                InsurancePolicyNumber = patient.InsurancePolicyNumber,
                RegistrationDate = patient.RegistrationDate
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Edit(string id, [FromForm] PatientProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var patient = await _userManager.FindByIdAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            // Update patient information
            patient.DateOfBirth = model.DateOfBirth;
            patient.Gender = model.Gender ?? patient.Gender;
            patient.Email = model.Email ?? patient.Email;
            patient.PhoneNumber = model.PhoneNumber ?? patient.PhoneNumber;
            patient.Address = model.Address ?? patient.Address;
            patient.City = model.City ?? patient.City;
            patient.State = model.State ?? patient.State;
            patient.ZipCode = model.ZipCode ?? patient.ZipCode;
            patient.EmergencyContactName = model.EmergencyContactName ?? patient.EmergencyContactName;
            patient.EmergencyContactPhone = model.EmergencyContactPhone ?? patient.EmergencyContactPhone;
            patient.EmergencyContactRelation = model.EmergencyContactRelation ?? patient.EmergencyContactRelation;
            patient.InsuranceProvider = model.InsuranceProvider ?? patient.InsuranceProvider;
            patient.InsurancePolicyNumber = model.InsurancePolicyNumber ?? patient.InsurancePolicyNumber;

            // Update audit fields
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                patient.LastModifiedById = currentUser.Id;
                patient.LastModifiedAt = DateTime.UtcNow;
            }

            var result = await _userManager.UpdateAsync(patient);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Patient information updated successfully.";
                return RedirectToAction(nameof(Details), new { id });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get upcoming appointments
            var upcomingAppointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == user.Id && a.AppointmentDate >= DateTime.Today)
                .OrderBy(a => a.AppointmentDate)
                .Take(5)
                .Select(a => new AppointmentViewModel
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    Purpose = a.Purpose,
                    DoctorName = $"Dr. {a.Doctor.FirstName} {a.Doctor.LastName}",
                    Status = a.Status
                })
                .ToListAsync();

            // Get recent visits
            var recentVisits = await _context.PatientVisits
                .Include(v => v.Doctor)
                .Where(v => v.PatientId == user.Id)
                .OrderByDescending(v => v.VisitDate)
                .Take(5)
                .Select(v => new PatientVisitViewModel
                {
                    Id = v.Id,
                    VisitDate = v.VisitDate,
                    Reason = v.Reason,
                    Diagnosis = v.Diagnosis,
                    DoctorName = $"Dr. {v.Doctor.FirstName} {v.Doctor.LastName}"
                })
                .ToListAsync();

            // Get test results
            var testResults = await _context.TestResults
                .Include(t => t.OrderedBy)
                .Where(t => t.PatientId == user.Id)
                .OrderByDescending(t => t.TestDate)
                .Take(5)
                .Select(t => new TestResultViewModel
                {
                    Id = t.Id,
                    TestName = t.TestName,
                    TestDate = t.TestDate,
                    Result = t.Result,
                    NormalRange = t.NormalRange,
                    Status = t.Status,
                    OrderedBy = $"Dr. {t.OrderedBy.FirstName} {t.OrderedBy.LastName}"
                })
                .ToListAsync();

            var viewModel = new PatientDashboardViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                FullName = $"{user.FirstName} {user.LastName}",
                MRN = user.MRN ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Address = user.Address ?? string.Empty,
                City = user.City ?? string.Empty,
                State = user.State ?? string.Empty,
                ZipCode = user.ZipCode ?? string.Empty,
                EmergencyContactName = user.EmergencyContactName ?? string.Empty,
                EmergencyContactPhone = user.EmergencyContactPhone ?? string.Empty,
                EmergencyContactRelation = user.EmergencyContactRelation ?? string.Empty,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender ?? string.Empty,
                UpcomingAppointments = upcomingAppointments,
                RecentVisits = recentVisits,
                TestResults = testResults
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateContactInfo(UpdateContactInfoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Dashboard));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            user.PhoneNumber = model.PhoneNumber;
            user.EmergencyContactName = model.EmergencyContactName;
            user.EmergencyContactPhone = model.EmergencyContactPhone;
            user.EmergencyContactRelation = model.EmergencyContactRelation;
            user.Address = model.Address;
            user.City = model.City;
            user.State = model.State;
            user.ZipCode = model.ZipCode;
            user.LastModifiedAt = DateTime.UtcNow;
            user.LastModifiedById = user.Id;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Contact information updated successfully.";
                return RedirectToAction(nameof(Dashboard));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestAppointment(AppointmentRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Dashboard));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var appointment = new Appointment
            {
                PatientId = user.Id,
                DoctorId = model.DoctorId,
                AppointmentDate = model.PreferredDate,
                Purpose = model.Purpose,
                Status = "Requested",
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment request submitted successfully.";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Appointments()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            // Get doctors for appointment requests
            var doctors = await _userManager.GetUsersInRoleAsync("Doctor");
            ViewBag.Doctors = doctors;

            // Get all appointments for the calendar
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == currentUser.Id)
                .OrderBy(a => a.AppointmentDate)
                .Select(a => new AppointmentViewModel
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    Purpose = a.Purpose,
                    DoctorName = $"Dr. {a.Doctor.FirstName} {a.Doctor.LastName}",
                    Status = a.Status
                })
                .ToListAsync();

            return View(appointments);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.PatientId == currentUser.Id);

            if (appointment == null)
            {
                return NotFound();
            }

            if (appointment.AppointmentDate <= DateTime.Now.AddHours(24))
            {
                return BadRequest("Appointments can only be cancelled at least 24 hours in advance.");
            }

            appointment.Status = "Cancelled";
            appointment.LastModifiedById = currentUser.Id;
            appointment.LastModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
} 