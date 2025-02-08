// Add these namespaces at the top
using EHRSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace EHRSystem.Data
{
    public class EhrDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public EhrDbContext(DbContextOptions<EhrDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<ApplicationUser>(b =>
            {
                b.Property(u => u.Id).HasMaxLength(85);
                
                // Add indexes for frequently searched fields
                b.HasIndex(u => new { u.FirstName, u.LastName });
                b.HasIndex(u => u.PhoneNumber);
                b.HasIndex(u => u.DateOfBirth);
                b.HasIndex(u => u.InsuranceProvider);
                b.HasIndex(u => u.RegistrationDate);
                b.HasIndex(u => u.MRN);
            });

            builder.Entity<ApplicationRole>(b =>
            {
                b.Property(r => r.Id).HasMaxLength(85);
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=EHRSystemDB;Trusted_Connection=True;",
                    b => b.MigrationsAssembly("EHRSystem.Data"));
            }
        }
    }
} 