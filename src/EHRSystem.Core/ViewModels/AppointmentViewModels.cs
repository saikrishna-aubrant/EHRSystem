using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EHRSystem.Core.ViewModels
{
    // [REQ: US-APT-01] View models for appointment scheduling

    public class AppointmentScheduleViewModel
    {
        // [REQ: US-APT-01.3] Doctor selection
        [Required(ErrorMessage = "Please select a doctor")]
        [Display(Name = "Doctor")]
        public string DoctorId { get; set; }

        [Display(Name = "Doctor List")]
        [ValidateNever]
        public List<SelectListItem> DoctorList { get; set; } = new();

        // [REQ: US-APT-01.4] Date and time selection
        [Required(ErrorMessage = "Please select a date")]
        [Display(Name = "Appointment Date")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Please select a time slot")]
        [Display(Name = "Time Slot")]
        public string TimeSlotId { get; set; }

        [Display(Name = "Available Time Slots")]
        [ValidateNever]
        public List<SelectListItem> AvailableTimeSlots { get; set; } = new();

        // [REQ: US-APT-01.5] Appointment details
        [Required(ErrorMessage = "Please provide a reason for the visit")]
        [StringLength(500)]
        [Display(Name = "Reason for Visit")]
        public string ReasonForVisit { get; set; }

        [ValidateNever]
        public string? PatientId { get; set; }

        [ValidateNever]
        public string? PatientName { get; set; }
    }

    public class CalendarViewModel
    {
        // [REQ: US-APT-01.6] Calendar view options
        public DateTime CurrentDate { get; set; }
        public string ViewType { get; set; } // daily/weekly/monthly
        public string UserRole { get; set; }
        public List<DoctorScheduleViewModel> DoctorSchedules { get; set; } = new();
        public List<string> WorkingHours { get; set; } = new();
    }

    public class DoctorScheduleViewModel
    {
        public string DoctorId { get; set; }
        public string DoctorName { get; set; }
        public List<TimeSlotViewModel> TimeSlots { get; set; } = new();
    }

    public class TimeSlotViewModel
    {
        public string Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsBufferTime { get; set; }
        public string AppointmentStatus { get; set; }
        public string PatientName { get; set; }
        public string Purpose { get; set; }
    }

    public class AppointmentConfirmationViewModel
    {
        // [REQ: US-APT-01.7] Confirmation details
        public string ReferenceNumber { get; set; }
        public string DoctorName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; }
        public string PatientName { get; set; }
        public string ReasonForVisit { get; set; }
    }
} 