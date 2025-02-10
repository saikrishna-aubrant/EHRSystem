# Authentication Module Test Cases

## US-AUTH-01: Role Management
### Test Case AUTH-01-01: Admin Role Management
**Requirement:** [REQ: US-AUTH-01.1] Admin panel for user role management
**Priority:** High
**Test Steps:**
1. Login as admin user
2. Navigate to Admin Panel
3. View list of users with their roles
4. Select a user and change their role
5. Save changes

**Expected Results:**
- User list should display correctly
- Role changes should be saved
- Audit trail should record the change

### Test Case AUTH-01-02: Role Assignment During Registration
**Requirement:** [REQ: US-AUTH-01.2] Admin user registration with role assignment
**Priority:** High
**Test Steps:**
1. Login as admin
2. Navigate to Admin Registration page
3. Fill registration form with role selection
4. Submit form

**Expected Results:**
- User should be created with selected role
- System should generate necessary credentials
- Welcome email should be sent

## US-AUTH-02: User Authentication
### Test Case AUTH-02-01: User Login
**Requirement:** [REQ: US-AUTH-02.6] Process user login
**Priority:** High
**Test Steps:**
1. Navigate to login page
2. Enter valid credentials
3. Click login button
4. Verify session timeout after 30 minutes

**Expected Results:**
- Successful login with valid credentials
- Failed login with invalid credentials
- Session timeout after 30 minutes of inactivity

### Test Case AUTH-02-02: Password Validation
**Requirement:** [REQ: US-AUTH-02.3] Process user registration
**Priority:** High
**Test Steps:**
1. Navigate to registration page
2. Test password requirements:
   - Less than 8 characters
   - No uppercase letter
   - No number
   - No special character

**Expected Results:**
- System should reject passwords that don't meet requirements
- Clear error messages should be displayed

## US-AUTH-03: Password Reset
### Test Case AUTH-03-01: Password Reset Request
**Requirement:** [REQ: US-AUTH-03.1] Password reset request
**Priority:** Medium
**Test Steps:**
1. Click "Forgot Password" link
2. Enter registered email
3. Submit request
4. Check email for reset link
5. Click reset link
6. Enter new password

**Expected Results:**
- Reset email should be sent
- Link should be valid for limited time
- Password should be updated successfully

### Test Case AUTH-03-02: Invalid Reset Attempts
**Requirement:** [REQ: US-AUTH-03.4] Process password reset
**Priority:** Medium
**Test Steps:**
1. Try to reset with invalid email
2. Try to use expired reset link
3. Try to use already used reset link

**Expected Results:**
- System should handle invalid attempts gracefully
- Clear error messages should be displayed
- Security measures should be enforced 