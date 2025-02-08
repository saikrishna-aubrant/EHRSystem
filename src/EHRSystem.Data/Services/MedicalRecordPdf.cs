using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using EHRSystem.Core.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.EntityFrameworkCore;

namespace EHRSystem.Data.Services
{
    public class MedicalRecordPdf
    {
        private readonly ApplicationUser _patient;
        private readonly EhrDbContext _context;

        public MedicalRecordPdf(ApplicationUser patient, EhrDbContext context)
        {
            _patient = patient;
            _context = context;
        }

        public async Task<byte[]> GeneratePdf()
        {
            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            try
            {
                // Add patient information
                document.Add(new Paragraph($"Medical Record for {_patient.FirstName} {_patient.LastName}"));
                document.Add(new Paragraph($"MRN: {_patient.MRN}"));
                document.Add(new Paragraph($"Date of Birth: {_patient.DateOfBirth:d}"));

                // Add medical history
                var medicalHistory = await _context.MedicalHistories
                    .Include(m => m.CreatedBy)
                    .Where(m => m.PatientId == _patient.Id)
                    .OrderByDescending(m => m.DiagnosedDate)
                    .ToListAsync();

                if (medicalHistory.Any())
                {
                    document.Add(new Paragraph("Medical History"));
                    foreach (var history in medicalHistory)
                    {
                        document.Add(new Paragraph($"- {history.Condition} (Diagnosed: {history.DiagnosedDate:d})"));
                        document.Add(new Paragraph($"  Treatment: {history.Treatment}"));
                    }
                }

                // Add medications
                var medications = await _context.Medications
                    .Include(m => m.PrescribedBy)
                    .Where(m => m.PatientId == _patient.Id && m.IsActive)
                    .OrderByDescending(m => m.PrescribedDate)
                    .ToListAsync();

                if (medications.Any())
                {
                    document.Add(new Paragraph("Current Medications"));
                    foreach (var medication in medications)
                    {
                        document.Add(new Paragraph($"- {medication.Name} {medication.Dosage}"));
                        document.Add(new Paragraph($"  Frequency: {medication.Frequency}"));
                        document.Add(new Paragraph($"  Prescribed by: {medication.PrescribedBy?.FirstName} {medication.PrescribedBy?.LastName}"));
                    }
                }

                // Add allergies
                var allergies = await _context.Allergies
                    .Where(a => a.PatientId == _patient.Id)
                    .ToListAsync();

                if (allergies.Any())
                {
                    document.Add(new Paragraph("Allergies"));
                    foreach (var allergy in allergies)
                    {
                        document.Add(new Paragraph($"- {allergy.AllergenName}"));
                        document.Add(new Paragraph($"  Reaction: {allergy.Reaction}"));
                        document.Add(new Paragraph($"  Severity: {allergy.Severity}"));
                    }
                }

                document.Close();
                return stream.ToArray();
            }
            catch
            {
                document.Close();
                throw;
            }
        }
    }
} 