using System;
using System.ComponentModel.DataAnnotations;

namespace EHRSystem.Core.Models
{
    public class TestResult
    {
        public int Id { get; set; }
        
        [Required]
        public string PatientId { get; set; }
        public ApplicationUser Patient { get; set; }
        
        [Required]
        public string TestName { get; set; }
        public DateTime TestDate { get; set; }
        public string Result { get; set; }
        public string NormalRange { get; set; }
        public string Status { get; set; }
        
        public string OrderedById { get; set; }
        public ApplicationUser OrderedBy { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public string? LastModifiedById { get; set; }
        public ApplicationUser LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }
} 