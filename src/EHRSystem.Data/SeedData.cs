using EHRSystem.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace EHRSystem.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<EhrDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            // Seed roles
            string[] roles = { "Admin", "Doctor", "Patient" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var applicationRole = new ApplicationRole(role)
                    {
                        Description = $"{role} role in the EHR system"
                    };
                    await roleManager.CreateAsync(applicationRole);
                }
            }

            // Seed admin user
            var adminEmail = "admin@ehrsystem.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Admin",
                    EmailConfirmed = true,
                    IsActive = true,
                    DateOfBirth = new DateTime(1980, 1, 1), // Default date
                    Gender = "Other",
                    MRN = "ADMIN0001"
                };

                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // Create a test doctor
            var testDoctor = new ApplicationUser
            {
                UserName = "doctor@test.com",
                Email = "doctor@test.com",
                FirstName = "John",
                LastName = "Smith",
                EmailConfirmed = true,
                PhoneNumber = "1234567890",
                IsActive = true
            };

            if (await userManager.FindByEmailAsync(testDoctor.Email) == null)
            {
                await userManager.CreateAsync(testDoctor, "Doctor123!");
                await userManager.AddToRoleAsync(testDoctor, "Doctor");
            }

            // Create a test patient with medical history
            var testPatient = new ApplicationUser
            {
                UserName = "patient@test.com",
                Email = "patient@test.com",
                FirstName = "Jane",
                LastName = "Doe",
                EmailConfirmed = true,
                PhoneNumber = "9876543210",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Female",
                Address = "123 Main St",
                City = "Springfield",
                State = "IL",
                ZipCode = "62701",
                EmergencyContactName = "John Doe",
                EmergencyContactPhone = "5555555555",
                EmergencyContactRelation = "Spouse",
                InsuranceProvider = "Blue Cross",
                InsurancePolicyNumber = "BC123456",
                MRN = "P" + DateTime.Now.ToString("yyyyMMdd") + "001",
                IsActive = true,
                RegistrationDate = DateTime.UtcNow
            };

            if (await userManager.FindByEmailAsync(testPatient.Email) == null)
            {
                var result = await userManager.CreateAsync(testPatient, "Patient123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(testPatient, "Patient");

                    // Add medical history
                    context.MedicalHistories.Add(new MedicalHistory
                    {
                        PatientId = testPatient.Id,
                        Condition = "Hypertension",
                        Description = "Stage 1 hypertension",
                        DiagnosedDate = DateTime.UtcNow.AddMonths(-6),
                        Treatment = "Lisinopril 10mg daily",
                        IsActive = true,
                        CreatedById = testDoctor.Id,
                        CreatedAt = DateTime.UtcNow
                    });

                    // Add medications
                    context.Medications.Add(new Medication
                    {
                        PatientId = testPatient.Id,
                        Name = "Lisinopril",
                        Dosage = "10mg",
                        Frequency = "Once daily",
                        PrescribedDate = DateTime.UtcNow.AddMonths(-6),
                        IsActive = true,
                        PrescribedById = testDoctor.Id,
                        CreatedAt = DateTime.UtcNow
                    });

                    // Add allergies
                    context.Allergies.Add(new Allergy
                    {
                        PatientId = testPatient.Id,
                        AllergenName = "Penicillin",
                        Reaction = "Rash",
                        Severity = "Moderate",
                        CreatedById = testDoctor.Id,
                        CreatedAt = DateTime.UtcNow
                    });

                    // Add a recent visit
                    context.PatientVisits.Add(new PatientVisit
                    {
                        PatientId = testPatient.Id,
                        DoctorId = testDoctor.Id,
                        VisitDate = DateTime.UtcNow.AddDays(-7),
                        Reason = "Follow-up for hypertension",
                        Diagnosis = "Controlled hypertension",
                        Treatment = "Continue current medication",
                        Notes = "Blood pressure 128/82",
                        CreatedAt = DateTime.UtcNow
                    });

                    await context.SaveChangesAsync();
                }
            }

            // Add test appointments
            if (!context.Appointments.Any())
            {
                var existingPatient = await userManager.FindByEmailAsync("patient@test.com");
                var existingDoctor = await userManager.FindByEmailAsync("doctor@test.com");
                
                if (existingPatient != null && existingDoctor != null)
                {
                    context.Appointments.AddRange(
                        new Appointment
                        {
                            PatientId = existingPatient.Id,
                            DoctorId = existingDoctor.Id,
                            AppointmentDate = DateTime.UtcNow.AddDays(7),
                            Purpose = "Follow-up visit",
                            Status = "Scheduled",
                            CreatedAt = DateTime.UtcNow
                        },
                        new Appointment
                        {
                            PatientId = existingPatient.Id,
                            DoctorId = existingDoctor.Id,
                            AppointmentDate = DateTime.UtcNow.AddDays(14),
                            Purpose = "Annual physical",
                            Status = "Scheduled",
                            CreatedAt = DateTime.UtcNow
                        }
                    );
                }
            }

            // Add test results
            if (!context.TestResults.Any())
            {
                var existingPatient = await userManager.FindByEmailAsync("patient@test.com");
                var existingDoctor = await userManager.FindByEmailAsync("doctor@test.com");
                
                if (existingPatient != null && existingDoctor != null)
                {
                    context.TestResults.AddRange(
                        new TestResult
                        {
                            PatientId = existingPatient.Id,
                            TestName = "Complete Blood Count",
                            TestDate = DateTime.UtcNow.AddDays(-5),
                            Result = "WBC: 7.5, RBC: 4.8, Hgb: 14.2",
                            NormalRange = "WBC: 4.5-11.0, RBC: 4.2-5.8, Hgb: 13.0-17.0",
                            Status = "Normal",
                            OrderedById = existingDoctor.Id,
                            CreatedAt = DateTime.UtcNow
                        },
                        new TestResult
                        {
                            PatientId = existingPatient.Id,
                            TestName = "Lipid Panel",
                            TestDate = DateTime.UtcNow.AddDays(-5),
                            Result = "Total Cholesterol: 185, HDL: 55, LDL: 110",
                            NormalRange = "Total: <200, HDL: >40, LDL: <130",
                            Status = "Normal",
                            OrderedById = existingDoctor.Id,
                            CreatedAt = DateTime.UtcNow
                        },
                        new TestResult
                        {
                            PatientId = existingPatient.Id,
                            TestName = "Blood Pressure",
                            TestDate = DateTime.UtcNow.AddDays(-1),
                            Result = "128/82",
                            NormalRange = "<120/80",
                            Status = "Elevated",
                            OrderedById = existingDoctor.Id,
                            CreatedAt = DateTime.UtcNow
                        }
                    );
                }
            }

            await context.SaveChangesAsync();
        }
    }
} 