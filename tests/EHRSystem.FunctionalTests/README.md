# EHR System Functional Test Suite

This directory contains manual test cases for the EHR System, organized by module and mapped to specific requirements.

## Test Structure

```
EHRSystem.FunctionalTests/
├── Authentication/
│   └── AuthenticationTests.md
├── PatientManagement/
│   └── PatientManagementTests.md
├── Appointments/
│   └── AppointmentTests.md
└── README.md
```

## Test Case Format
Each test case follows this structure:
- Test Case ID
- Requirement Reference
- Priority
- Test Steps
- Expected Results

## Priority Levels
- **High**: Critical functionality, must be tested in every cycle
- **Medium**: Important features, should be tested in most cycles
- **Low**: Nice-to-have features, can be tested less frequently

## Modules Covered

### 1. Authentication Module (US-AUTH)
- Role Management
- User Authentication
- Password Reset

### 2. Patient Management Module (US-PAT)
- Patient Registration
- Patient Search
- Profile Management
- Patient Portal

### 3. Appointments Module (US-APT)
- Schedule Appointments
- Appointment Reminders
- Reschedule/Cancel Appointments

## Test Execution Guidelines

1. **Pre-requisites**
   - Test environment setup
   - Test data available
   - Required user accounts created

2. **Test Execution**
   - Follow test steps in order
   - Document actual results
   - Note any deviations
   - Capture screenshots for issues

3. **Issue Reporting**
   - Use standard bug template
   - Include test case reference
   - Attach relevant screenshots
   - Document reproduction steps

4. **Test Cycle**
   - Run high priority tests in every cycle
   - Include medium priority based on changes
   - Run low priority in full regression

## Maintenance

- Update test cases when requirements change
- Add new test cases for new features
- Archive obsolete test cases
- Maintain traceability to requirements 