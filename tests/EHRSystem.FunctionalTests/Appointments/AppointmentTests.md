# Appointment Module Test Cases

## US-APT-01: Schedule Appointments
### Test Case APT-01-01: Basic Appointment Scheduling
**Requirement:** [REQ: US-APT-01] Schedule appointments
**Priority:** High
**Test Steps:**
1. Login as patient/staff
2. Navigate to appointment scheduling
3. Select doctor
4. Choose available date
5. Select time slot
6. Enter visit reason
7. Confirm appointment

**Expected Results:**
- Available slots shown correctly
- No double booking possible
- Confirmation received
- Email notification sent

### Test Case APT-01-02: Calendar View
**Requirement:** [REQ: US-APT-01.5] Calendar functionality
**Priority:** High
**Test Steps:**
1. View calendar in different modes:
   - Daily view
   - Weekly view
   - Monthly view
2. Check slot availability
3. Verify time slot duration
4. Check buffer times

**Expected Results:**
- Calendar displays correctly
- 30-minute slot duration enforced
- 15-minute buffer time between appointments
- Working hours (9 AM - 5 PM) enforced

### Test Case APT-01-03: Scheduling Validation
**Requirement:** [REQ: US-APT-01] Scheduling rules
**Priority:** High
**Test Steps:**
1. Try to schedule outside working hours
2. Attempt double booking
3. Test minimum notice period
4. Verify slot duration limits

**Expected Results:**
- Proper validation messages
- No appointments outside 9 AM - 5 PM
- No overlapping appointments
- Minimum notice period enforced

## US-APT-02: Appointment Reminders
### Test Case APT-02-01: Reminder Generation
**Requirement:** [REQ: US-APT-02] Send appointment reminders
**Priority:** Medium
**Test Steps:**
1. Schedule appointment
2. Wait for 24-hour reminder
3. Verify reminder content
4. Check all notification channels

**Expected Results:**
- Reminder sent 24 hours before
- Correct appointment details in reminder
- All notification channels working

### Test Case APT-02-02: Reminder Management
**Requirement:** [REQ: US-APT-02] Reminder preferences
**Priority:** Low
**Test Steps:**
1. Update notification preferences
2. Disable reminders
3. Re-enable reminders
4. Change reminder timing

**Expected Results:**
- Preferences saved correctly
- Reminders follow preferences
- Changes take effect immediately

## US-APT-03: Reschedule/Cancel Appointments
### Test Case APT-03-01: Appointment Rescheduling
**Requirement:** [REQ: US-APT-03] Reschedule appointments
**Priority:** High
**Test Steps:**
1. Select existing appointment
2. Choose new date/time
3. Provide reason for rescheduling
4. Confirm changes
5. Check notifications

**Expected Results:**
- Original slot freed up
- New slot booked
- History maintained
- Notifications sent
- Audit trail updated

### Test Case APT-03-02: Appointment Cancellation
**Requirement:** [REQ: US-APT-03] Cancel appointments
**Priority:** High
**Test Steps:**
1. Select appointment to cancel
2. Provide cancellation reason
3. Confirm cancellation
4. Check slot availability
5. Verify notifications

**Expected Results:**
- Slot freed up immediately
- Cancellation recorded
- Notifications sent
- Audit trail updated

### Test Case APT-03-03: Late Changes
**Requirement:** [REQ: US-APT-03] Change restrictions
**Priority:** Medium
**Test Steps:**
1. Attempt late rescheduling (<24h)
2. Try late cancellation (<24h)
3. Test admin override
4. Check notifications

**Expected Results:**
- Changes blocked within 24h
- Admin override works
- Proper notifications sent
- Audit trail maintained 