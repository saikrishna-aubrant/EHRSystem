using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EHRSystem.Core.ViewModels
{
    // [REQ: US-APT-01] View models for appointment scheduling and management

    // [REQ: US-APT-01.1] Appointment Scheduling Form Model
    public class AppointmentScheduleViewModel
    {
        // [REQ: US-APT-01.2] Doctor Selection
        [Required(ErrorMessage = "Please select a doctor")]
        [Display(Name = "Doctor")]
        public string DoctorId { get; set; }

        [Display(Name = "Doctor List")]
        [ValidateNever]
        public List<SelectListItem> DoctorList { get; set; } = new();

        // [REQ: US-APT-01.3] Date and Time Selection
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

        // [REQ: US-APT-01.4] Visit Details
        [Required(ErrorMessage = "Please provide a reason for the visit")]
        [StringLength(500)]
        [Display(Name = "Reason for Visit")]
        public string ReasonForVisit { get; set; }

        [ValidateNever]
        public string? PatientId { get; set; }

        [ValidateNever]
        public string? PatientName { get; set; }
    }

    // [REQ: US-APT-01.5] Calendar View Model
    public class CalendarViewModel
    {
        public DateTime CurrentDate { get; set; }
        public string ViewType { get; set; } // daily/weekly/monthly
        public string UserRole { get; set; }
        public List<DoctorScheduleViewModel> DoctorSchedules { get; set; } = new();
        public List<string> WorkingHours { get; set; } = new();
    }

    // [REQ: US-APT-01.6] Doctor Schedule View Model
    public class DoctorScheduleViewModel
    {
        public string DoctorId { get; set; }
        public string DoctorName { get; set; }
        public List<TimeSlotViewModel> TimeSlots { get; set; } = new();
    }

    // [REQ: US-APT-01.7] Time Slot View Model
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

    // [REQ: US-APT-02] Notification View Models
    public class AppointmentReminderViewModel
    {
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; }
        public string Location { get; set; }
        public string ContactNumber { get; set; }
    }

    // [REQ: US-APT-03] Rescheduling View Models
    public class RescheduleViewModel
    {
        public int AppointmentId { get; set; }
        public DateTime CurrentDateTime { get; set; }
        public DateTime NewDateTime { get; set; }
        public string Reason { get; set; }
        public List<SelectListItem> AvailableSlots { get; set; } = new();
    }
} 