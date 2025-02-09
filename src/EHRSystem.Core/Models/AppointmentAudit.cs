using System;

namespace EHRSystem.Core.Models
{
    public class AppointmentAudit
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public string Action { get; set; }
        public string Reason { get; set; }
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
        public DateTime? OldDateTime { get; set; }
        public DateTime? NewDateTime { get; set; }
        public string ModifiedById { get; set; }
        public DateTime ModifiedAt { get; set; }

        // Navigation properties
        public virtual Appointment Appointment { get; set; }
        public virtual ApplicationUser ModifiedBy { get; set; }
    }
} 