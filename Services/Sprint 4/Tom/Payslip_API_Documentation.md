# Payslip API Documentation

## Overview
The Payslip API provides endpoints for managing payslips in the Farm Time Management system. It allows you to calculate total hours worked, create payslips, query payslip data, and delete payslips.

## Base URL
```
https://localhost:7000/api/payslip
```

## Authentication
Currently no authentication is required. (May be added in future sprints)

---

## Endpoints

### 1. Get Total Hours Worked

**Endpoint:** `GET /api/payslip/totalhoursworked/{staffId}`

**Description:** Calculates and returns the total hours worked by a staff member for a specific week.

**Parameters:**
- `staffId` (int, required): The ID of the staff member
- `weekStartDate` (string, optional): Any date within the week in `yyyy-MM-dd` format. If not provided, uses current date.

**Example Request:**
```http
GET /api/payslip/totalhoursworked/1?weekStartDate=2024-01-15
```

**Response:**
- **Success (200):** Returns a decimal number representing total hours worked
- **Bad Request (400):** Invalid staff ID or date format
- **Server Error (500):** Internal server error

**Example Response:**
```
38.5
```

**Logic:**
- Retrieves all WorkSchedule entries for the week
- Retrieves all Event entries for the week
- Adds scheduled hours for each WorkSchedule
- Deducts 0.5 hours for each "Break" event within schedule time ranges
- Returns the final calculated total hours

---

### 2. Create Payslip

**Endpoint:** `POST /api/payslip/create`

**Description:** Creates a new payslip for a staff member for a specific week, or returns existing payslip if already exists.

**Request Body:**
```json
{
    "StaffId": 1,
    "WeekStartDate": "2024-01-15T00:00:00"
}
```

**Parameters:**
- `StaffId` (int, required): The ID of the staff member
- `WeekStartDate` (DateTime, required): Any date within the week to calculate

**Example Request:**
```http
POST /api/payslip/create
Content-Type: application/json

{
    "StaffId": 1,
    "WeekStartDate": "2024-01-15T00:00:00"
}
```

**Response:**
- **Success (200):** Returns the created or existing Payslip object
- **Bad Request (400):** Invalid request body or missing required fields
- **Server Error (500):** Internal server error

**Example Response:**
```json
{
    "payslipId": 1,
    "staffId": 1,
    "standardPayRate": 25.00,
    "weekStartDate": "2024-01-15T00:00:00",
    "totalHoursWorked": 38.5,
    "grossWeeklyPay": 962.50,
    "annualIncome": 50050.00,
    "annualTax": 5087.20,
    "weeklyPAYG": 97.83,
    "netPay": 864.67,
    "employerSuperannuation": 115.50,
    "dateCreated": "2024-01-20T10:30:00"
}
```

**Logic:**
- Gets staff information including StandardPayRate
- Calculates total hours worked using CalculateTotalHourWorker
- Checks if payslip already exists for the week
- If exists, returns existing payslip
- If not exists, calculates all payroll values and creates new payslip

---

### 3. Delete Payslip

**Endpoint:** `DELETE /api/payslip/{payslipId}`

**Description:** Deletes a payslip by its ID.

**Parameters:**
- `payslipId` (int, required): The ID of the payslip to delete

**Example Request:**
```http
DELETE /api/payslip/1
```

**Response:**
- **Success (200):** Returns the deleted Payslip object
- **Bad Request (400):** Invalid payslip ID
- **Not Found (404):** Payslip not found
- **Server Error (500):** Internal server error

**Example Response:**
```json
{
    "payslipId": 1,
    "staffId": 1,
    "standardPayRate": 25.00,
    "weekStartDate": "2024-01-15T00:00:00",
    "totalHoursWorked": 38.5,
    "grossWeeklyPay": 962.50,
    "annualIncome": 50050.00,
    "annualTax": 5087.20,
    "weeklyPAYG": 97.83,
    "netPay": 864.67,
    "employerSuperannuation": 115.50,
    "dateCreated": "2024-01-20T10:30:00"
}
```

---

### 4. Get All Payslips

**Endpoint:** `GET /api/payslip`

**Description:** Retrieves all payslips in the system.

**Example Request:**
```http
GET /api/payslip
```

**Response:**
- **Success (200):** Returns an array of all Payslip objects
- **Server Error (500):** Internal server error

**Example Response:**
```json
[
    {
        "payslipId": 1,
        "staffId": 1,
        "standardPayRate": 25.00,
        "weekStartDate": "2024-01-15T00:00:00",
        "totalHoursWorked": 38.5,
        "grossWeeklyPay": 962.50,
        "annualIncome": 50050.00,
        "annualTax": 5087.20,
        "weeklyPAYG": 97.83,
        "netPay": 864.67,
        "employerSuperannuation": 115.50,
        "dateCreated": "2024-01-20T10:30:00"
    },
    {
        "payslipId": 2,
        "staffId": 2,
        "standardPayRate": 30.00,
        "weekStartDate": "2024-01-15T00:00:00",
        "totalHoursWorked": 40.0,
        "grossWeeklyPay": 1200.00,
        "annualIncome": 62400.00,
        "annualTax": 7952.00,
        "weeklyPAYG": 152.92,
        "netPay": 1047.08,
        "employerSuperannuation": 144.00,
        "dateCreated": "2024-01-20T11:15:00"
    }
]
```

---

### 5. Get Payslips by Staff ID

**Endpoint:** `GET /api/payslip/staff/{staffId}`

**Description:** Retrieves all payslips for a specific staff member.

**Parameters:**
- `staffId` (int, required): The ID of the staff member

**Example Request:**
```http
GET /api/payslip/staff/1
```

**Response:**
- **Success (200):** Returns an array of Payslip objects for the staff member
- **Bad Request (400):** Invalid staff ID
- **Server Error (500):** Internal server error

**Example Response:**
```json
[
    {
        "payslipId": 1,
        "staffId": 1,
        "standardPayRate": 25.00,
        "weekStartDate": "2024-01-15T00:00:00",
        "totalHoursWorked": 38.5,
        "grossWeeklyPay": 962.50,
        "annualIncome": 50050.00,
        "annualTax": 5087.20,
        "weeklyPAYG": 97.83,
        "netPay": 864.67,
        "employerSuperannuation": 115.50,
        "dateCreated": "2024-01-20T10:30:00"
    }
]
```

---

## Data Models

### Payslip Object
```json
{
    "payslipId": "integer (auto-generated)",
    "staffId": "integer (required)",
    "standardPayRate": "decimal (required)",
    "weekStartDate": "DateTime (required)",
    "totalHoursWorked": "decimal (calculated)",
    "grossWeeklyPay": "decimal (calculated)",
    "annualIncome": "decimal (calculated)",
    "annualTax": "decimal (calculated)",
    "weeklyPAYG": "decimal (calculated)",
    "netPay": "decimal (calculated)",
    "employerSuperannuation": "decimal (calculated)",
    "dateCreated": "DateTime (auto-generated)"
}
```

### Create Payslip Request
```json
{
    "StaffId": "integer (required)",
    "WeekStartDate": "DateTime (required)"
}
```

---

## Business Logic

### Hours Calculation Logic
1. **WorkSchedule Integration:** Retrieves all WorkSchedule entries for the specified week
2. **Event Integration:** Retrieves all Event entries for the specified week
3. **Break Deduction:** For each WorkSchedule, deducts 0.5 hours for each "Break" event that occurs within the schedule's time range
4. **Total Calculation:** Sums all scheduled hours minus break deductions

### Payroll Calculation Formulas
- **Gross Weekly Pay:** `TotalHoursWorked × StandardPayRate`
- **Annual Income:** `GrossWeeklyPay × 52`
- **Annual Tax:** Calculated using ATO tax brackets
- **Weekly PAYG:** `AnnualTax ÷ 52`
- **Net Pay:** `GrossWeeklyPay - WeeklyPAYG`
- **Employer Superannuation:** `GrossWeeklyPay × 12%`

### Week Logic
- **Week Start:** Any date input is converted to the Monday of that week
- **Week Range:** Monday 00:00:00 to Sunday 23:59:59
- **Date Format:** Supports `yyyy-MM-dd` format for weekStartDate parameter

---

## Error Handling

### Common Error Responses

**400 Bad Request:**
```json
{
    "message": "StaffId must be greater than 0"
}
```

**404 Not Found:**
```json
{
    "message": "Payslip not found"
}
```

**500 Internal Server Error:**
```json
{
    "message": "Error details"
}
```

---

## Testing

### Using HTTP Test File
Use the provided `test_payslip_endpoints.http` file to test all endpoints:

```http
### Test with Staff ID 1
GET https://localhost:7000/api/payslip/totalhoursworked/1?weekStartDate=2024-01-15

### Create payslip for Staff ID 1
POST https://localhost:7000/api/payslip/create
Content-Type: application/json

{
    "StaffId": 1,
    "WeekStartDate": "2024-01-15T00:00:00"
}
```

### Sample Test Scenarios

1. **Calculate hours for current week:**
   ```
   GET /api/payslip/totalhoursworked/1
   ```

2. **Calculate hours for specific week:**
   ```
   GET /api/payslip/totalhoursworked/1?weekStartDate=2024-01-15
   ```

3. **Create payslip and retrieve existing:**
   ```
   POST /api/payslip/create (first time - creates new)
   POST /api/payslip/create (second time - returns existing)
   ```

4. **Query and filter payslips:**
   ```
   GET /api/payslip (all payslips)
   GET /api/payslip/staff/1 (specific staff)
   ```

---

## Notes

- **Date Handling:** All dates are handled in the server's local timezone
- **Decimal Precision:** All monetary and hour values are stored with appropriate decimal precision
- **Idempotent Creation:** Creating a payslip for the same staff and week returns the existing payslip
- **Break Events:** Only events with EventType = "Break" are considered for hour deduction
- **Tax Calculation:** Uses current ATO tax brackets (may need updates for different tax years)

---

## Version History

- **Sprint 4:** Initial implementation of Payslip API
  - Basic CRUD operations
  - Hours calculation with break deduction
  - Payroll calculation with ATO tax brackets
  - Week-based filtering logic
