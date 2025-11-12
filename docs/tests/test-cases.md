# Test Cases

## User Registration Tests

| Test Case ID | Scenario | Action | Expected Result | Actual Result | Assessment |
|:---|:---|:---|:---|:---|:---|
| REG-001 | Successful user registration | Call Register with valid data | User is created in repository with hashed password | User created | Test passed |
| REG-002 | Registration with invalid data | Call Register with invalid email and short password | ValidationException is thrown | ValidationException thrown | Test passed |
| REG-003 | Required fields validation | Check user validator | All fields pass validation | Validation passed | Test passed |

## User Login Tests

| Test Case ID | Scenario | Action | Expected Result | Actual Result | Assessment |
|:---|:---|:---|:---|:---|:---|
| LOGIN-001 | Successful login | Call Login with correct credentials | JWT tokens are returned, tokens are stored in cookies | Tokens returned and stored | Test passed |
| LOGIN-002 | Login with wrong password | Call Login with incorrect password | AuthenticationException is thrown | AuthenticationException thrown | Test passed |
| LOGIN-003 | Login with non-existent email | Call Login with non-existent email | AuthenticationException is thrown | AuthenticationException thrown | Test passed |

## User Logout Tests

| Test Case ID | Scenario | Action | Expected Result | Actual Result | Assessment |
|:---|:---|:---|:---|:---|:---|
| LOGOUT-001 | Successful logout | Call Logout with valid token | Refresh token invalidated, cookies cleared | Token invalidated | Test passed |
| LOGOUT-002 | Logout with invalid token | Call Logout with invalid token | InvalidOperationException is thrown | InvalidOperationException thrown | Test passed |

## Repository Tests

| Test Case ID | Scenario | Action | Expected Result | Actual Result | Assessment |
|:---|:---|:---|:---|:---|:---|
| REPO-001 | Find user by ID | Call GetUserByIdAsync with existing ID | User returned with related events | User with events returned | Test passed |
| REPO-002 | Find non-existent user | Call GetUserByIdAsync with non-existent ID | Returns null | null returned | Test passed |
| REPO-003 | Create user | Call CreateUserAsync with valid data | User saved to database | User saved | Test passed |