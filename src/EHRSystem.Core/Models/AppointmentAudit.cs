using System;

namespace EHRSystem.Core.Models
{
    // [REQ: US-APT-03] AppointmentAudit Model - Tracks appointment changes and cancellations
    public class AppointmentAudit
    {
        public int Id { get; set; }
        
        // [REQ: US-APT-03.1] Appointment Reference
        public int AppointmentId { get; set; }
        
        // [REQ: US-APT-03.2] Change Action Details
        public string Action { get; set; }
        public string Reason { get; set; }
        
        // [REQ: US-APT-03.3] Status Change History
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
        
        // [REQ: US-APT-03.4] Schedule Change History
        public DateTime? OldDateTime { get; set; }
        public DateTime? NewDateTime { get; set; }
        
        // [REQ: US-APT-03.5] Change Tracking Details
        public string ModifiedById { get; set; }
        public DateTime ModifiedAt { get; set; }

        // [REQ: US-APT-03.6] Related Entities
        public virtual Appointment Appointment { get; set; }
        public virtual ApplicationUser ModifiedBy { get; set; }
    }
} 