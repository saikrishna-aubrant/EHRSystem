using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EHRSystem.Core.ViewModels;
using EHRSystem.Core.Models;
using EHRSystem.Data;
using System.Linq.Dynamic.Core;

namespace EHRSystem.Web.Controllers
{
    [Authorize(Roles = "Admin,Doctor")]
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

        public IActionResult Search()
        {
            return View(new PatientSearchViewModel());
        }

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
    }
} 