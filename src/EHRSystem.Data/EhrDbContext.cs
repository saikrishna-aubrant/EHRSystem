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

        public DbSet<MedicalHistory> MedicalHistories { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<Allergy> Allergies { get; set; }
        public DbSet<PatientVisit> PatientVisits { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<TestResult> TestResults { get; set; }

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

                // Configure LastModifiedBy relationship
                b.HasOne(u => u.LastModifiedBy)
                 .WithMany()
                 .HasForeignKey(u => u.LastModifiedById)
                 .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<MedicalHistory>(b =>
            {
                b.HasOne(m => m.Patient)
                 .WithMany()
                 .HasForeignKey(m => m.PatientId)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(m => m.CreatedBy)
                 .WithMany()
                 .HasForeignKey(m => m.CreatedById)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(m => m.LastModifiedBy)
                 .WithMany()
                 .HasForeignKey(m => m.LastModifiedById)
                 .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<Medication>(b =>
            {
                b.HasOne(m => m.Patient)
                 .WithMany()
                 .HasForeignKey(m => m.PatientId)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(m => m.PrescribedBy)
                 .WithMany()
                 .HasForeignKey(m => m.PrescribedById)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(m => m.LastModifiedBy)
                 .WithMany()
                 .HasForeignKey(m => m.LastModifiedById)
                 .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<Allergy>(b =>
            {
                b.HasOne(a => a.Patient)
                 .WithMany()
                 .HasForeignKey(a => a.PatientId)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(a => a.CreatedBy)
                 .WithMany()
                 .HasForeignKey(a => a.CreatedById)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(a => a.LastModifiedBy)
                 .WithMany()
                 .HasForeignKey(a => a.LastModifiedById)
                 .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<PatientVisit>(b =>
            {
                b.HasOne(v => v.Patient)
                 .WithMany()
                 .HasForeignKey(v => v.PatientId)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(v => v.Doctor)
                 .WithMany()
                 .HasForeignKey(v => v.DoctorId)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(v => v.LastModifiedBy)
                 .WithMany()
                 .HasForeignKey(v => v.LastModifiedById)
                 .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<ApplicationRole>(b =>
            {
                b.Property(r => r.Id).HasMaxLength(85);
            });

            builder.Entity<Appointment>(b =>
            {
                b.HasOne(a => a.Patient)
                 .WithMany()
                 .HasForeignKey(a => a.PatientId)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(a => a.Doctor)
                 .WithMany()
                 .HasForeignKey(a => a.DoctorId)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(a => a.LastModifiedBy)
                 .WithMany()
                 .HasForeignKey(a => a.LastModifiedById)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasIndex(a => a.AppointmentDate);
                b.HasIndex(a => a.Status);
            });

            builder.Entity<TestResult>(b =>
            {
                b.HasOne(t => t.Patient)
                 .WithMany()
                 .HasForeignKey(t => t.PatientId)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(t => t.OrderedBy)
                 .WithMany()
                 .HasForeignKey(t => t.OrderedById)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(t => t.LastModifiedBy)
                 .WithMany()
                 .HasForeignKey(t => t.LastModifiedById)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasIndex(t => t.TestDate);
                b.HasIndex(t => t.TestName);
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