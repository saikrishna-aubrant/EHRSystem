using EHRSystem.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace EHRSystem.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Seed Roles
            string[] roleNames = { "Admin", "Doctor", "Nurse", "Patient" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new ApplicationRole(roleName) 
                    {
                        Description = $"{roleName} role"
                    });
                }
            }

            // 2. Seed Admin User
            var adminEmail = "admin@ehr.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true
                };
                
                Console.WriteLine($"Seeding admin user: {adminEmail}");
                var createResult = await userManager.CreateAsync(adminUser, "Admin@1234!");
                Console.WriteLine($"Create result: {createResult.Succeeded}");
                
                if (!createResult.Succeeded)
                {
                    throw new Exception($"Admin user creation failed: {string.Join(", ", createResult.Errors)}");
                }
                
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
} 