# Testing Plan

## Contents

- [Testing Plan](#testing-plan)
  - [Contents](#contents)
  - [Introduction](#introduction)
  - [Test Items](#test-items)
  - [Quality Attributes](#quality-attributes)
  - [Risks](#risks)
  - [Testing Aspects](#testing-aspects)
    - [User Registration](#user-registration)
    - [User Authentication](#user-authentication)
    - [User Logout](#user-logout)
    - [Repository Operations](#repository-operations)
  - [Testing Approaches](#testing-approaches)
  - [Results Representation](#results-representation)
  - [Conclusions](#conclusions)

## Introduction

This document outlines the testing plan for the "EventTestTask" application. It is intended for individuals responsible for testing this project. The goal of the testing is to verify whether the actual behavior of the application aligns with its expected behavior as described in the requirements.

## Test Items

The following functional requirements are identified as test items:

- User registration
- User authentication
- User logout
- User data validation
- JWT tokens generation
- Password hashing
- User repository operations
- Refresh tokens management

## Quality Attributes

1. **Functionality**:
    - Functional Completeness: The application should perform all declared functions
    - Functional Accuracy: The application should perform all declared functions correctly

2. **Reliability**:
    - Handling of invalid data
    - Resilience to error scenarios

3. **Security**:
    - Secure password storage
    - Proper JWT tokens handling

## Risks

The following risks are identified:
- Database connection failures
- User data validation issues
- JWT tokens generation errors
- Password hashing problems

## Testing Aspects

The testing will focus on verifying the implementation of the application's core functions. Aspects to be tested include:

### User Registration
Testing must ensure:
- Successful user registration with valid data
- Proper error handling for invalid data
- Correct password hashing
- Validation of all required fields

### User Authentication
Testing must ensure:
- Successful login with correct credentials
- Proper handling of incorrect passwords
- Proper handling of non-existent emails
- JWT tokens generation
- Tokens storage in cookies

### User Logout
Testing must ensure:
- Proper tokens removal
- Refresh token invalidation
- Cookies cleanup

### Repository Operations
Testing must ensure:
- User creation in database
- User retrieval by ID
- User retrieval by email
- Loading of related data (events)

## Testing Approaches

A combined approach will be used:
- Unit testing of services
- Integration testing of repositories
- Mocking dependencies with Moq
- Using InMemory database for repository testing

## Results Representation

Results will be documented in the "Testing Results" report.

## Conclusions

This testing plan enables the verification of the core functionality of the application. While passing all tests does not guarantee full compatibility across all platform versions and architectures, it provides confidence that the software performs as intended.