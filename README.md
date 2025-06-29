# Customer Account Management System (CAMS)

## Overview

This project is a robust Customer Account Management System (CAMS) built with ASP.NET Core 9.0. It provides a comprehensive solution for managing customer accounts, transactions, and related banking operations. The system is designed with a clean architecture, promoting separation of concerns, maintainability, and scalability.

## Key Features

*   **User and Client Management:** Secure registration, authentication, and management of users and their associated client profiles.
*   **Account Management:** Creation, modification, and management of various account types (Current, Savings, Fixed Deposit, Loan, Joint Accounts).
*   **Transaction Processing:** Handling of different transaction types (deposits, withdrawals, transfers) with audit logging.
*   **Role-Based Access Control:** Granular permissions based on user roles.
*   **Email Notifications:** Automated email notifications for account activities.
*   **Background Job Processing:** Utilizes Hangfire for scheduling and processing recurring tasks (e.g., monthly limit resets).
*   **API Documentation:** Integrated Swagger/OpenAPI for easy API exploration and testing.
*   **Soft Deletion:** Entities are soft-deleted to maintain data integrity and historical records.
*   **Audit Logging:** Comprehensive auditing of entity changes and user actions.

## Entity Relationships

The core entities and their relationships are as follows:

*   **`AppUser` (IdentityUser):** Represents a system user. Each `AppUser` can be associated with one `Client`.
*   **`Client`:** Represents a customer. A `Client` has personal details, an `Address`, and is linked to an `AppUser`. A `Client` can have multiple `Accounts`.
*   **`Account`:** An abstract base class for different account types (Current, Savings, Fixed Deposit, Loan, Joint Account). An `Account` belongs to a `Client` and can have multiple `Transactions`.
*   **`Transaction`:** Represents a financial transaction associated with an `Account`.
*   **`Address`:** A value object containing address details, embedded within the `Client` entity.
*   **`AuditEntry`:** Records changes to entities and user actions for auditing purposes.
*   **`PasswordResetCode`:** Stores codes for password reset functionality.

### Relationships Summary:

*   `AppUser` 1 : 1 `Client`
*   `Client` 1 : M `Account`
*   `Account` 1 : M `Transaction`

## Technologies Used

*   **.NET 9.0:** The core framework for building the application.
*   **ASP.NET Core Web API:** For building RESTful APIs.
*   **Entity Framework Core:** ORM for data access and database interactions.
*   **ASP.NET Core Identity:** For user authentication and authorization.
*   **AutoMapper:** For object-to-object mapping.
*   **FluentValidation:** For request validation.
*   **Serilog:** For structured logging.
*   **BCrypt.Net-Core:** For secure password hashing.
*   **MailKit:** For sending emails.
*   **Hangfire:** For background job processing and scheduling.
*   **Swagger/OpenAPI:** For API documentation and testing.

## Technical Highlights

*   **Clean Architecture:** The project follows a clean architecture approach, separating concerns into distinct layers (API, Business, Core, Domains, Infrastructure, DependencyInjection, Shared).
*   **Repository Pattern & Unit of Work:** Implemented for abstracting data access and ensuring transactional consistency.
*   **Dependency Injection:** Extensively used for managing dependencies and promoting loose coupling.
*   **Middleware:** Custom middleware for error handling, request timing, and permission-based authorization.
*   **Soft Deletion:** Entities implement `ISoftDeletable` interface, allowing logical deletion instead of physical deletion, preserving historical data.
*   **Audit Trail:** `AppDbContext` overrides `SaveChangesAsync` to automatically log changes to entities, including creation, modification, and soft deletion, along with user and IP address information.
*   **Background Jobs with Hangfire:** Demonstrates the use of Hangfire for scheduling recurring tasks, such as resetting monthly limits for accounts.
*   **Fluent API for Entity Configuration:** Entity configurations are applied using `builder.ApplyConfigurationsFromAssembly` in `AppDbContext` for cleaner model configuration.
*   **Custom Serilog Integration:** (Commented out but present) Shows an example of integrating Serilog for comprehensive logging.

## Key Use Cases

*   **Client Onboarding:** Registering new clients and associating them with user accounts.
*   **Account Opening:** Creating various types of bank accounts for clients.
*   **Financial Transactions:** Performing deposits, withdrawals, and transfers between accounts.
*   **Account Status Management:** Changing account statuses (e.g., active, closed).
*   **Password Reset:** Securely resetting user passwords.
*   **Automated Monthly Resets:** Background jobs automatically reset monthly transaction limits for certain account types.
*   **Auditing:** Tracking all significant changes to client and account data for compliance and historical analysis.

# API Documentation

This document provides detailed information about the RESTful APIs exposed by the Customer Account Management System (CAMS).

## Accounts API

### GET /api/Accounts/client/{clientId}

**Description:** Retrieves a paginated list of accounts for a specific client, with optional search functionality.

**Permissions:** `Accounts.ViewAccountsForClient`

**Parameters:**

*   `clientId` (string, FromRoute): The unique identifier of the client.
*   `query` (AccountQuery, FromQuery): Query parameters for pagination and searching.
    *   `PageNumber` (int): The page number to retrieve.
    *   `PageSize` (int): The number of items per page.
    *   `SearchBy` (string, optional): Text to search by (e.g., AccountNumber, AccountStatus).

**Response:**

```json
{
  "items": [
    {
      "id": "string",
      "createdAt": "2025-06-29T12:00:00Z",
      "createdByUserId": "string",
      "updatedAt": "2025-06-29T12:00:00Z",
      "updatedByUserId": "string",
      "isDeleted": false,
      "accountNumber": "string",
      "clientId": "string",
      "currencyType": "USD",
      "balance": 0,
      "branch": "string",
      "accountStatus": "Active",
      "isSigned": false
    }
  ],
  "pageNumber": 0,
  "pageSize": 0,
  "totalPages": 0
}
```

### GET /api/Accounts/{accountId}

**Description:** Retrieves details of a specific account by its ID.

**Permissions:** `Accounts.ViewById`

**Parameters:**

*   `accountId` (string, FromRoute): The unique identifier of the account.

**Response:**

```json
{
  "id": "string",
  "createdAt": "2025-06-29T12:00:00Z",
  "createdByUserId": "string",
  "updatedAt": "2025-06-29T12:00:00Z",
  "updatedByUserId": "string",
  "isDeleted": false,
  "accountNumber": "string",
  "clientId": "string",
  "currencyType": "USD",
  "balance": 0,
  "branch": "string",
  "accountStatus": "Active",
  "isSigned": false,
  "transactions": []
}
```

### PUT /api/Accounts/{accountId}/status

**Description:** Changes the status of a specific account.

**Permissions:** `Accounts.ChangeAccountStatus`

**Parameters:**

*   `accountId` (string, FromRoute): The unique identifier of the account.
*   `newStatus` (AccountStatus, FromQuery): The new status for the account (e.g., `Active`, `Closed`, `Suspended`).

**Response:**

*   `204 No Content` on success.

### POST /api/Accounts/{accountId}/flag-signed

**Description:** Flags an account as signed.

**Permissions:** `Accounts.FlagAccountSigned`

**Parameters:**

*   `accountId` (string, FromRoute): The unique identifier of the account.

**Response:**

*   `204 No Content` on success.

### POST /api/Accounts/{accountId}/close

**Description:** Closes a specific account.

**Permissions:** `Accounts.CloseAccount`

**Parameters:**

*   `accountId` (string, FromRoute): The unique identifier of the account.

**Response:**

*   `204 No Content` on success.





## Authentications API

### POST /api/Authentications/login

**Description:** Authenticates a user and returns a JWT.

**Permissions:** `AllowAnonymousPermission`

**Request Body:**

```json
{
  "email": "string",
  "password": "string"
}
```

**Response:**

```json
{
  "token": "string",
  "expiration": "2025-06-29T12:00:00Z"
}
```

### POST /api/Authentications/request-reset

**Description:** Requests a password reset code for a given email.

**Permissions:** `AllowAnonymousPermission`

**Request Body:**

```json
{
  "email": "string"
}
```

**Response:**

```json
{
  "message": "If account exists, reset code has been sent."
}
```

### POST /api/Authentications/verify-reset-code

**Description:** Verifies a password reset code and returns a temporary token.

**Permissions:** `AllowAnonymousPermission`

**Request Body:**

```json
{
  "email": "string",
  "code": "string"
}
```

**Response:**

```json
{
  "token": "string"
}
```

### POST /api/Authentications/reset-password

**Description:** Resets the user's password using a temporary token and new password.

**Permissions:** `AllowAnonymousPermission`

**Request Body:**

```json
{
  "token": "string",
  "newPassword": "string"
}
```

**Response:**

```json
{
  "message": "Password reset successful."
}
```




## Clients API

### POST /api/Clients

**Description:** Registers a new client in the system.

**Permissions:** `Clients.RegisterClient`

**Request Body (multipart/form-data):**

```
{
  "fullName": "string",
  "ssn": "string",
  "image": "file",
  "country": "string",
  "city": "string",
  "street": "string",
  "zipCode": "string",
  "nationality": "string",
  "gender": "Male" | "Female",
  "birthDate": "YYYY-MM-DD",
  "jobTitle": "string",
  "monthlyIncome": 0,
  "financialSource": 0,
  "email": "string",
  "password": "string",
  "userType": "Client"
}
```

**Response:**

```json
{
  "id": "string",
  "fullName": "string",
  "ssn": "string",
  "imagePath": "string",
  "address": {
    "country": "string",
    "city": "string",
    "street": "string",
    "zipCode": "string"
  },
  "nationality": "string",
  "gender": "Male",
  "birthDate": "YYYY-MM-DD",
  "jobTitle": "string",
  "monthlyIncome": 0,
  "financialSource": 0,
  "userId": "string"
}
```

### GET /api/Clients/{clientId}

**Description:** Retrieves details of a specific client by their ID.

**Permissions:** `Clients.ViewById`

**Parameters:**

*   `clientId` (string, FromRoute): The unique identifier of the client.

**Response:**

```json
{
  "id": "string",
  "fullName": "string",
  "ssn": "string",
  "imagePath": "string",
  "address": {
    "country": "string",
    "city": "string",
    "street": "string",
    "zipCode": "string"
  },
  "nationality": "string",
  "gender": "Male",
  "birthDate": "YYYY-MM-DD",
  "jobTitle": "string",
  "monthlyIncome": 0,
  "financialSource": 0,
  "userId": "string"
}
```

### GET /api/Clients

**Description:** Retrieves a paginated list of clients.

**Permissions:** `Clients.View`

**Parameters:**

*   `query` (ClientQuery, FromQuery): Query parameters for pagination and searching.
    *   `PageNumber` (int): The page number to retrieve.
    *   `PageSize` (int): The number of items per page.
    *   `SearchBy` (string, optional): Text to search by (e.g., FullName, SSN).

**Response:**

```json
{
  "items": [
    {
      "id": "string",
      "fullName": "string",
      "ssn": "string",
      "imagePath": "string",
      "address": {
        "country": "string",
        "city": "string",
        "street": "string",
        "zipCode": "string"
      },
      "nationality": "string",
      "gender": "Male",
      "birthDate": "YYYY-MM-DD",
      "jobTitle": "string",
      "monthlyIncome": 0,
      "financialSource": 0,
      "userId": "string"
    }
  ],
  "pageNumber": 0,
  "pageSize": 0,
  "totalPages": 0
}
```

### PUT /api/Clients/{clientId}

**Description:** Updates an existing client's details.

**Permissions:** `Clients.Edit`

**Parameters:**

*   `clientId` (string, FromRoute): The unique identifier of the client to update.

**Request Body (multipart/form-data):**

```
{
  "fullName": "string",
  "ssn": "string",
  "image": "file" (optional),
  "country": "string",
  "city": "string",
  "street": "string",
  "zipCode": "string",
  "nationality": "string",
  "gender": "Male" | "Female",
  "birthDate": "YYYY-MM-DD",
  "jobTitle": "string",
  "monthlyIncome": 0,
  "financialSource": 0
}
```

**Response:**

*   `204 No Content` on success.

### DELETE /api/Clients/{clientId}

**Description:** Soft deletes a client by their ID.

**Permissions:** `Clients.Soft`

**Parameters:**

*   `clientId` (string, FromRoute): The unique identifier of the client to soft delete.

**Response:**

*   `204 No Content` on success.

### POST /api/Clients/{clientId}/restore

**Description:** Restores a soft-deleted client by their ID.

**Permissions:** `Clients.Restore`

**Parameters:**

*   `clientId` (string, FromRoute): The unique identifier of the client to restore.

**Response:**

*   `204 No Content` on success.




## Currents API

### POST /api/Currents/open

**Description:** Opens a new current account for a client.

**Permissions:** `Currents.OpenCurrent`

**Request Body:**

```json
{
  "clientId": "string",
  "currencyType": "USD" | "EUR" | "GBP",
  "balance": 0,
  "branch": "string",
  "transactionLimit": 0,
  "monthlyFee": 0
}
```

**Response:**

```json
{
  "id": "string",
  "createdAt": "2025-06-29T12:00:00Z",
  "createdByUserId": "string",
  "updatedAt": "2025-06-29T12:00:00Z",
  "updatedByUserId": "string",
  "isDeleted": false,
  "accountNumber": "string",
  "clientId": "string",
  "currencyType": "USD",
  "balance": 0,
  "branch": "string",
  "accountStatus": "Active",
  "isSigned": false,
  "transactionLimit": 0,
  "monthlyFee": 0
}
```

### GET /api/Currents

**Description:** Retrieves a paginated list of current accounts.

**Permissions:** `Currents.View`

**Parameters:**

*   `query` (CurrentQuery, FromQuery): Query parameters for pagination and searching.
    *   `PageNumber` (int): The page number to retrieve.
    *   `PageSize` (int): The number of items per page.
    *   `SearchBy` (string, optional): Text to search by (e.g., AccountNumber, AccountStatus).

**Response:**

```json
{
  "items": [
    {
      "id": "string",
      "createdAt": "2025-06-29T12:00:00Z",
      "createdByUserId": "string",
      "updatedAt": "2025-06-29T12:00:00Z",
      "updatedByUserId": "string",
      "isDeleted": false,
      "accountNumber": "string",
      "clientId": "string",
      "currencyType": "USD",
      "balance": 0,
      "branch": "string",
      "accountStatus": "Active",
      "isSigned": false,
      "transactionLimit": 0,
      "monthlyFee": 0
    }
  ],
  "pageNumber": 0,
  "pageSize": 0,
  "totalPages": 0
}
```

### GET /api/Currents/{currentId}

**Description:** Retrieves details of a specific current account by its ID.

**Permissions:** `Currents.ViewById`

**Parameters:**

*   `currentId` (string, FromRoute): The unique identifier of the current account.

**Response:**

```json
{
  "id": "string",
  "createdAt": "2025-06-29T12:00:00Z",
  "createdByUserId": "string",
  "updatedAt": "2025-06-29T12:00:00Z",
  "updatedByUserId": "string",
  "isDeleted": false,
  "accountNumber": "string",
  "clientId": "string",
  "currencyType": "USD",
  "balance": 0,
  "branch": "string",
  "accountStatus": "Active",
  "isSigned": false,
  "transactionLimit": 0,
  "monthlyFee": 0,
  "transactions": []
}
```




## Fixed Deposits API

### POST /api/FixedDeposits/open

**Description:** Opens a new fixed deposit account for a client.

**Permissions:** `FixedDeposits.OpenFixedDeposit`

**Request Body:**

```json
{
  "clientId": "string",
  "currencyType": "USD" | "EUR" | "GBP",
  "balance": 0,
  "branch": "string",
  "interestRate": 0,
  "maturityDate": "YYYY-MM-DDTHH:MM:SSZ"
}
```

**Response:**

```json
{
  "id": "string",
  "createdAt": "2025-06-29T12:00:00Z",
  "createdByUserId": "string",
  "updatedAt": "2025-06-29T12:00:00Z",
  "updatedByUserId": "string",
  "isDeleted": false,
  "accountNumber": "string",
  "clientId": "string",
  "currencyType": "USD",
  "balance": 0,
  "branch": "string",
  "accountStatus": "Active",
  "isSigned": false,
  "interestRate": 0,
  "maturityDate": "2025-06-29T12:00:00Z"
}
```

### GET /api/FixedDeposits

**Description:** Retrieves a paginated list of fixed deposit accounts.

**Permissions:** `FixedDeposits.View`

**Parameters:**

*   `query` (FixedDepositQuery, FromQuery): Query parameters for pagination and searching.
    *   `PageNumber` (int): The page number to retrieve.
    *   `PageSize` (int): The number of items per page.
    *   `SearchBy` (string, optional): Text to search by (e.g., AccountNumber, AccountStatus).

**Response:**

```json
{
  "items": [
    {
      "id": "string",
      "createdAt": "2025-06-29T12:00:00Z",
      "createdByUserId": "string",
      "updatedAt": "2025-06-29T12:00:00Z",
      "updatedByUserId": "string",
      "isDeleted": false,
      "accountNumber": "string",
      "clientId": "string",
      "currencyType": "USD",
      "balance": 0,
      "branch": "string",
      "accountStatus": "Active",
      "isSigned": false,
      "interestRate": 0,
      "maturityDate": "2025-06-29T12:00:00Z"
    }
  ],
  "pageNumber": 0,
  "pageSize": 0,
  "totalPages": 0
}
```




## Joint Accounts API

### POST /api/JointAccounts

**Description:** Creates a new joint account for the authenticated client.

**Permissions:** `JointAccounts.OpenJointAccount`

**Request Body:**

```json
{
  "currencyType": "USD" | "EUR" | "GBP",
  "balance": 0,
  "branch": "string"
}
```

**Response:**

*   `202 Accepted` on success.

### POST /api/JointAccounts/{jointAccountId}/secondary-holder

**Description:** Adds a secondary holder to an existing joint account.

**Permissions:** `JointAccounts.AddSecondary`

**Parameters:**

*   `jointAccountId` (string, FromRoute): The unique identifier of the joint account.

**Request Body:**

```json
{
  "secondaryHolderClientId": "string"
}
```

**Response:**

*   `204 No Content` on success.

### DELETE /api/JointAccounts/{jointAccountId}/secondary-holder/{secondaryClientId}

**Description:** Removes a secondary holder from a joint account.

**Permissions:** `JointAccounts.RemoveSecondary`

**Parameters:**

*   `jointAccountId` (string, FromRoute): The unique identifier of the joint account.
*   `secondaryClientId` (string, FromRoute): The unique identifier of the secondary client to remove.

**Response:**

*   `204 No Content` on success.

### GET /api/JointAccounts

**Description:** Retrieves a paginated list of joint accounts.

**Permissions:** `JointAccounts.View`

**Parameters:**

*   `query` (JointAccountQuery, FromQuery): Query parameters for pagination and searching.
    *   `PageNumber` (int): The page number to retrieve.
    *   `PageSize` (int): The number of items per page.
    *   `SearchBy` (string, optional): Text to search by (e.g., AccountNumber, AccountStatus).

**Response:**

```json
{
  "items": [
    {
      "id": "string",
      "createdAt": "2025-06-29T12:00:00Z",
      "createdByUserId": "string",
      "updatedAt": "2025-06-29T12:00:00Z",
      "updatedByUserId": "string",
      "isDeleted": false,
      "accountNumber": "string",
      "clientId": "string",
      "currencyType": "USD",
      "balance": 0,
      "branch": "string",
      "accountStatus": "Active",
      "isSigned": false,
      "secondaryHolders": [
        {
          "id": "string",
          "fullName": "string"
        }
      ]
    }
  ],
  "pageNumber": 0,
  "pageSize": 0,
  "totalPages": 0
}
```




## Loans API

### POST /api/Loans/apply

**Description:** Allows a client to apply for a new loan.

**Permissions:** `Loans.Apply`

**Request Body:**

```json
{
  "clientId": "string",
  "currencyType": "USD" | "EUR" | "GBP",
  "balance": 0,
  "branch": "string",
  "loanAmount": 0,
  "interestRate": 0,
  "loanTermMonths": 0
}
```

**Response:**

*   `202 Accepted` on success.

### POST /api/Loans/{loanAccountId}/approve

**Description:** Approves a loan application.

**Permissions:** `Loans.Approve`

**Parameters:**

*   `loanAccountId` (string, FromRoute): The unique identifier of the loan account to approve.

**Response:**

*   `204 No Content` on success.

### POST /api/Loans/{loanId}/installment

**Description:** Records an installment payment for a loan.

**Permissions:** `Loans.Installment`

**Parameters:**

*   `loanId` (string, FromRoute): The unique identifier of the loan.
*   `amount` (decimal, FromQuery): The amount of the installment payment.
*   `sourceAccountId` (string, FromQuery): The ID of the account from which the payment is made.

**Response:**

*   `204 No Content` on success.

### GET /api/Loans

**Description:** Retrieves a paginated list of loan accounts.

**Permissions:** `Loans.View`

**Parameters:**

*   `query` (LoanQuery, FromQuery): Query parameters for pagination and searching.
    *   `PageNumber` (int): The page number to retrieve.
    *   `PageSize` (int): The number of items per page.
    *   `SearchBy` (string, optional): Text
(Content truncated due to size limit. Use line ranges to read in chunks)

