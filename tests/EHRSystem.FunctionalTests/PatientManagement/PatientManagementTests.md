# Patient Management Module Test Cases

## US-PAT-01: Patient Registration
### Test Case PAT-01-01: New Patient Registration
**Requirement:** [REQ: US-PAT-01] Patient registration with demographics
**Priority:** High
**Test Steps:**
1. Navigate to patient registration page
2. Fill in required fields:
   - Full Name (First, Middle, Last)
   - Date of Birth
   - Gender
   - Contact Information
   - Address
   - Emergency Contact
3. Submit registration form

**Expected Results:**
- Patient record created successfully
- Unique MRN generated
- Confirmation message displayed
- Welcome email sent to patient

### Test Case PAT-01-02: Validation Checks
**Requirement:** [REQ: US-PAT-01] Field validation
**Priority:** High
**Test Steps:**
1. Submit form with missing required fields
2. Enter invalid date of birth (future date)
3. Enter invalid phone number format
4. Enter invalid email format
5. Test ZIP code validation

**Expected Results:**
- Clear error messages for each validation failure
- Form should not submit with invalid data
- Valid data should be preserved when errors occur

## US-PAT-02: Patient Search
### Test Case PAT-02-01: Basic Search
**Requirement:** [REQ: US-PAT-02] Patient search functionality
**Priority:** High
**Test Steps:**
1. Navigate to patient search
2. Search by:
   - Patient ID
   - Name (partial)
   - Date of Birth
   - Phone Number
3. Verify search results

**Expected Results:**
- Results display within 1 second
- Correct matching records shown
- Pagination working for large result sets
- 20 records per page

### Test Case PAT-02-02: Advanced Search
**Requirement:** [REQ: US-PAT-02] Advanced search options
**Priority:** Medium
**Test Steps:**
1. Use advanced search filters:
   - Insurance Provider
   - Registration Date Range
2. Sort results by different columns
3. Export search results

**Expected Results:**
- Filters applied correctly
- Sorting working for all columns
- Export functionality working

## US-PAT-03: Profile Management
### Test Case PAT-03-01: View Profile
**Requirement:** [REQ: US-PAT-03] Patient profile viewing
**Priority:** High
**Test Steps:**
1. Access patient profile
2. Verify displayed information:
   - Demographics
   - Medical History
   - Recent Visits
   - Active Medications
   - Allergies
   - Insurance

**Expected Results:**
- All information displayed correctly
- Access controls enforced by role
- History properly organized

### Test Case PAT-03-02: Edit Profile
**Requirement:** [REQ: US-PAT-03] Profile updates
**Priority:** High
**Test Steps:**
1. Edit patient information:
   - Update contact details
   - Modify emergency contacts
   - Update insurance information
2. Save changes
3. View audit trail

**Expected Results:**
- Changes saved successfully
- Audit trail updated
- Change history maintained

## US-PAT-04: Patient Portal
### Test Case PAT-04-01: Portal Access
**Requirement:** [REQ: US-PAT-04] Patient portal functionality
**Priority:** High
**Test Steps:**
1. Login to patient portal
2. View:
   - Personal information
   - Upcoming appointments
   - Recent visits
   - Test results
3. Test mobile responsiveness

**Expected Results:**
- All information accessible
- Portal works on mobile devices
- Secure access maintained

### Test Case PAT-04-02: Self-Service Features
**Requirement:** [REQ: US-PAT-04] Patient self-service
**Priority:** Medium
**Test Steps:**
1. Update contact information
2. Request appointment
3. Send message to provider
4. Download medical records

**Expected Results:**
- Updates processed correctly
- Appointments requested successfully
- Messages sent securely
- Records downloaded in proper format 