# Pay Rate Configuration API Documentation
**For Front-End Developers**

## Overview
This API manages pay rate configurations for farm staff using the existing Staff table. It supports different roles (Worker/Manager) and contract types (Full-time/Part-time/Casual) with Horticulture Award 2025 compliant rates.

## Base URL
```
/api/payrates
```

## Authentication
All `PUT` and `POST` endpoints require:
- **JWT Token** in Authorization header: `Bearer <token>`
- **Admin Role** - Only admin users can update pay rates

## API Endpoints

### 1. Get All Staff with Pay Rates
```http
GET /api/payrates/staff
```
**Description**: Retrieves all staff with their current pay rates
**Auth Required**: No
**Response**: Array of Staff objects with pay rate information

**Example Response**:
```json
[
  {
    "staffId": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@farm.com",
    "phone": "0412345678",
    "address": "123 Farm Road",
    "contractType": "Full-time",
    "role": "Worker",
    "standardHoursPerWeek": 38.0,
    "standardPayRate": 25.00,
    "overtimePayRate": 37.50,
    "password": null
  }
]
```

### 2. Get Staff by Role and Contract Type
```http
GET /api/payrates/staff/{role}/{contractType}
```
**Description**: Retrieves staff members with specific role and contract type
**Parameters**:
- `role`: "Worker" or "Manager"
- `contractType`: "Full-time", "Part-time", or "Casual"

**Example**:
```http
GET /api/payrates/staff/Worker/Full-time
```

**Response**: Array of Staff objects matching the criteria

### 3. Get Specific Staff Member
```http
GET /api/payrates/staff/{id}
```
**Description**: Retrieves a specific staff member by ID with pay rates
**Auth Required**: No
**Response**: Single Staff object or 404 if not found

### 4. Get Default Pay Rate for Role/Contract
```http
GET /api/payrates/defaults/{role}/{contractType}
```
**Description**: Gets the default Horticulture Award rate for a specific combination
**Parameters**:
- `role`: "Worker" or "Manager"
- `contractType`: "Full-time", "Part-time", or "Casual"

**Example**:
```http
GET /api/payrates/defaults/Worker/Casual
```

**Response**:
```json
{
  "standardRate": 31.25,
  "overtimeRate": 46.87,
  "weekendRate": 62.50
}
```

### 5. Get All Default Pay Rates
```http
GET /api/payrates/defaults
```
**Description**: Gets all default Horticulture Award 2025 rates
**Auth Required**: No

**Response**:
```json
{
  "Worker": {
    "Full-time": {
      "standardRate": 25.00,
      "overtimeRate": 37.50,
      "weekendRate": 50.00
    },
    "Casual": {
      "standardRate": 31.25,
      "overtimeRate": 46.87,
      "weekendRate": 62.50
    }
  },
  "Manager": {
    "Full-time": {
      "standardRate": 35.00,
      "overtimeRate": 52.50,
      "weekendRate": 70.00
    }
  }
}
```

### 6. Update Individual Staff Pay Rates
```http
PUT /api/payrates/staff/{id}/payrates
Authorization: Bearer <jwt-token>
Content-Type: application/json
```
**Description**: Updates pay rates for a specific staff member (Admin only)
**Auth Required**: Yes (Admin role)

**Request Body**:
```json
{
  "standardPayRate": 26.00,
  "overtimePayRate": 39.00
}
```

**Response**:
```json
{
  "message": "Staff pay rates updated successfully",
  "staff": "{...updated staff object...}"
}
```

### 7. Bulk Update Pay Rates by Role/Contract
```http
PUT /api/payrates/bulk/{role}/{contractType}
Authorization: Bearer <jwt-token>
Content-Type: application/json
```
**Description**: Updates pay rates for all staff with specific role and contract type (Admin only)
**Auth Required**: Yes (Admin role)

**Example**:
```http
PUT /api/payrates/bulk/Worker/Full-time
```

**Request Body**:
```json
{
  "standardPayRate": 26.00,
  "overtimePayRate": 39.00
}
```

**Response**:
```json
{
  "message": "Successfully updated 5 staff members with role 'Worker' and contract type 'Full-time'",
  "staff": "[...array of updated staff...]"
}
```

### 8. Initialize Default Pay Rates
```http
POST /api/payrates/initialize-defaults
Authorization: Bearer <jwt-token>
```
**Description**: Updates all staff with default Horticulture Award 2025 rates (Admin only)
**Auth Required**: Yes (Admin role)

**Response**:
```json
{
  "message": "Successfully updated 12 staff members with default Horticulture Award 2025 pay rates",
  "staff": "[...array of updated staff...]"
}
```

## Data Model

### Staff Object (with Pay Rate Information)
```typescript
interface Staff {
  staffId: number;             // Primary key
  firstName: string;           // Staff first name
  lastName: string;            // Staff last name
  email?: string;              // Email address (optional)
  phone?: string;              // Phone number (optional)
  address?: string;            // Address (optional)
  contractType?: string;       // "Full-time" | "Part-time" | "Casual"
  role?: string;               // "Worker" | "Manager" | "Admin"
  standardHoursPerWeek?: number; // Standard weekly hours
  standardPayRate?: number;    // Standard hourly rate (weekday)
  overtimePayRate?: number;    // Overtime hourly rate
  password?: string;           // Password hash (hidden in responses)
}
```

### PayRateInfo Object (for defaults)
```typescript
interface PayRateInfo {
  standardRate: number;        // Base hourly rate
  overtimeRate: number;        // Overtime rate (1.5x base)
  weekendRate: number;         // Weekend rate (2x base) - calculated property
}
```

### PayRateUpdateRequest Object
```typescript
interface PayRateUpdateRequest {
  standardPayRate: number;     // New standard hourly rate
  overtimePayRate: number;     // New overtime hourly rate
}
```

## Default Rate Structure
*Based on Horticulture Award 2025*

| Role | Contract Type | Weekday | Weekend/Holiday | Overtime |
|------|---------------|---------|-----------------|----------|
| Worker | Full-time/Part-time | $25.00 | $50.00 (2x) | $37.50 (1.5x) |
| Worker | Casual | $31.25 (+25%) | $62.50 (2x + 25%) | $46.87 (1.5x + 25%) |
| Manager | Full-time/Part-time | $35.00 | $70.00 (2x) | $52.50 (1.5x) |
| Manager | Casual | $43.75 (+25%) | $87.50 (2x + 25%) | $65.62 (1.5x + 25%) |

## Error Responses

### 401 Unauthorized
```json
{
  "message": "Authentication required"
}
```

### 403 Forbidden
```json
"Only admin users can create pay rates"
```

### 404 Not Found
```json
{
  "message": "PayRate not found"
}
```

### 400 Bad Request
```json
{
  "message": "Failed to create pay rate"
}
```

## Business Rules for Frontend

### Staff Pay Rate Management
- Pay rates are stored directly in the Staff table (`standardPayRate`, `overtimePayRate`)
- Weekend rates are calculated as 2x the standard rate
- Changes are applied immediately to staff records

### Role & Contract Validation
- **Roles**: "Worker", "Manager", or "Admin"
- **Contract Types**: "Full-time", "Part-time", or "Casual"
- Rates must be positive numbers with max 2 decimal places
- Use default rates as starting point for new configurations

### Admin Access Control
- Only users with `role: "Admin"` can update pay rates
- Display create/edit UI only for admin users
- Handle 403 Forbidden responses gracefully
- Individual staff updates vs bulk updates by role/contract

### Default Rate Logic
- Worker rates: $25/h standard, $37.50/h overtime (1.5x), $50/h weekend (2x)
- Manager rates: $35/h standard, $52.50/h overtime (1.5x), $70/h weekend (2x)
- Casual loading: +25% on all rates (Worker Casual: $31.25/h standard)

## Frontend Implementation Examples

### React Hook for Pay Rates
```javascript
const usePayRates = () => {
  const [payRates, setPayRates] = useState([]);
  const [loading, setLoading] = useState(true);

  const fetchActiveRates = async () => {
    try {
      const response = await fetch('/api/payrates/active');
      const data = await response.json();
      setPayRates(JSON.parse(data));
    } catch (error) {
      console.error('Failed to fetch pay rates:', error);
    } finally {
      setLoading(false);
    }
  };

  const createPayRate = async (payRate, token) => {
    const response = await fetch('/api/payrates', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(payRate)
    });

    if (!response.ok) {
      throw new Error(`Failed to create pay rate: ${response.status}`);
    }

    return await response.json();
  };

  return { payRates, loading, fetchActiveRates, createPayRate };
};
```

### Form Validation Example
```javascript
const validatePayRate = (payRate) => {
  const errors = {};

  if (!['Worker', 'Manager'].includes(payRate.role)) {
    errors.role = 'Role must be Worker or Manager';
  }

  if (!['Full-time', 'Part-time', 'Casual'].includes(payRate.contractType)) {
    errors.contractType = 'Invalid contract type';
  }

  if (payRate.weekdayRate <= 0) {
    errors.weekdayRate = 'Weekday rate must be greater than 0';
  }

  return errors;
};
```

## Testing Endpoints

Use tools like Postman or curl to test:

```bash
# Get active rates
curl -X GET http://localhost:5000/api/payrates/active

# Create new rate (requires admin JWT)
curl -X POST http://localhost:5000/api/payrates \
  -H "Authorization: Bearer <your-admin-jwt>" \
  -H "Content-Type: application/json" \
  -d '{"role":"Worker","contractType":"Full-time","weekdayRate":25.00,"weekendRate":50.00,"overtimeRate":37.50}'
```

## Database Schema Reference

### PayRate Table Structure
```sql
CREATE TABLE [PayRate] (
    [PayRateId] int IDENTITY(1,1) PRIMARY KEY,
    [Role] nvarchar(50) NOT NULL,
    [ContractType] nvarchar(50) NOT NULL,
    [WeekdayRate] decimal(10,2) NOT NULL,
    [WeekendRate] decimal(10,2) NOT NULL,
    [OvertimeRate] decimal(10,2) NOT NULL,
    [EffectiveFromDate] datetime2 NOT NULL,
    [EffectiveToDate] datetime2 NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedByStaffId] int NOT NULL
);
```

---

This documentation provides everything your frontend developers need to implement the pay rate configuration UI correctly.