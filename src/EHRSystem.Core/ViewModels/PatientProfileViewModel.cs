namespace EHRSystem.Core.ViewModels
{
    public class PatientProfileViewModel
    {
        // Basic Info
        public string Id { get; set; }
        public string FullName { get; set; }
        public string MRN { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        
        // Contact Info
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        
        // Emergency Contact
        public string EmergencyContactName { get; set; }
        public string EmergencyContactPhone { get; set; }
        public string EmergencyContactRelation { get; set; }
        
        // Insurance
        public string InsuranceProvider { get; set; }
        public string InsurancePolicyNumber { get; set; }
        
        // Medical Info
        public List<MedicalHistoryViewModel> MedicalHistory { get; set; } = new List<MedicalHistoryViewModel>();
        public List<MedicationViewModel> Medications { get; set; } = new List<MedicationViewModel>();
        public List<AllergyViewModel> Allergies { get; set; } = new List<AllergyViewModel>();
        public List<PatientVisitViewModel> RecentVisits { get; set; } = new List<PatientVisitViewModel>();
        
        // Audit Info
        public DateTime RegistrationDate { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string LastModifiedBy { get; set; }
    }

    public class MedicalHistoryViewModel
    {
        public int Id { get; set; }
        public string Condition { get; set; }
        public string Description { get; set; }
        public DateTime DiagnosedDate { get; set; }
        public string Treatment { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MedicationViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Dosage { get; set; }
        public string Frequency { get; set; }
        public DateTime PrescribedDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public string PrescribedBy { get; set; }
    }

    public class AllergyViewModel
    {
        public int Id { get; set; }
        public string AllergenName { get; set; }
        public string Reaction { get; set; }
        public string Severity { get; set; }
    }

    public class PatientVisitViewModel
    {
        public int Id { get; set; }
        public DateTime VisitDate { get; set; }
        public string Reason { get; set; }
        public string Diagnosis { get; set; }
        public string Treatment { get; set; }
        public string DoctorName { get; set; }
    }
} 