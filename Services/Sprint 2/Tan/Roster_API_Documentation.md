# Roster (WorkSchedule) API Documentation
**For Front-End Developers**

## Overview
This API manages staff work schedules and roster assignments. It provides comprehensive functionality for:
- Viewing rosters (with worker-level security)
- Assigning shifts (admin only)
- Date range filtering (current week, upcoming weeks, specific weeks)
- Automatic hours calculation
- Overlapping shift prevention
- Audit logging

**Sprint:** Sprint 2
**Developer:** Tan
**Compliance:** Worker-level access control, real-time roster viewing support

---

## Base URL
```
/api/roster
```

---

## Authentication
Most endpoints require JWT Bearer token authentication.

### Authorization Levels:
- **Public**: No authentication required (limited endpoints)
- **Authenticated Worker**: Requires valid JWT token - Workers can ONLY view their own roster
- **Admin Only**: Requires JWT token with Admin role - Can view/manage all rosters

---

## API Endpoints

### 📋 **GET Operations (Roster Viewing)**

---

### 1. Get Current Week Schedule ⭐ **RECOMMENDED FOR WORKERS**
```http
GET /api/roster/staff/{staffId}/current-week
```

**Description**: Returns the worker's schedule for the current week (Monday to Sunday).

**Auth Required**: Yes (Bearer Token)

**Access Control**: Workers can only view their own roster. Admins can view any roster.

**Request Example:**
```bash
GET /api/roster/staff/2/current-week
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response (200 OK):**
```json
[
  {
    "scheduleId": 1,
    "staffId": 2,
    "startTime": "2025-10-16T08:00:00",
    "endTime": "2025-10-16T16:00:00",
    "scheduleHours": 8
  },
  {
    "scheduleId": 2,
    "staffId": 2,
    "startTime": "2025-10-17T09:00:00",
    "endTime": "2025-10-17T17:00:00",
    "scheduleHours": 8
  }
]
```

**Error (401 Unauthorized):**
```json
{
  "message": "Workers can only view their own roster"
}
```

**Use Case:** Dashboard - "My Schedule This Week"

---

### 2. Get Upcoming Schedules ⭐ **RECOMMENDED FOR WORKERS**
```http
GET /api/roster/staff/{staffId}/upcoming?weeks={numberOfWeeks}
```

**Description**: Returns upcoming schedules for the next N weeks (default: 4 weeks from today).

**Auth Required**: Yes (Bearer Token)

**Access Control**: Workers can only view their own roster. Admins can view any roster.

**Query Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `weeks` | integer | No | `4` | Number of weeks ahead (1-52) |

**Request Examples:**
```bash
# Get next 4 weeks (default)
GET /api/roster/staff/2/upcoming
Authorization: Bearer {token}

# Get next 8 weeks
GET /api/roster/staff/2/upcoming?weeks=8
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
[
  {
    "scheduleId": 3,
    "staffId": 2,
    "startTime": "2025-10-18T08:00:00",
    "endTime": "2025-10-18T16:00:00",
    "scheduleHours": 8
  },
  {
    "scheduleId": 4,
    "staffId": 2,
    "startTime": "2025-10-25T08:00:00",
    "endTime": "2025-10-25T16:00:00",
    "scheduleHours": 8
  }
]
```

**Error (400 Bad Request):**
```json
{
  "message": "Weeks parameter must be between 1 and 52"
}
```

**Use Case:** "View Upcoming Weeks" feature

---

### 3. Get Specific Week Schedule ⭐ **RECOMMENDED FOR WORKERS**
```http
GET /api/roster/staff/{staffId}/week?date={yyyy-MM-dd}
```

**Description**: Returns schedule for a specific week (Monday to Sunday). Pass any date in the target week, and the API returns the entire week's schedule.

**Auth Required**: Yes (Bearer Token)

**Access Control**: Workers can only view their own roster. Admins can view any roster.

**Query Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `date` | string (yyyy-MM-dd) | No | Today | Any date in the desired week |

**Request Examples:**
```bash
# Get current week (no date parameter)
GET /api/roster/staff/2/week
Authorization: Bearer {token}

# Get week containing October 20, 2025
GET /api/roster/staff/2/week?date=2025-10-20
Authorization: Bearer {token}

# Get week containing November 15, 2025
GET /api/roster/staff/2/week?date=2025-11-15
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
[
  {
    "scheduleId": 5,
    "staffId": 2,
    "startTime": "2025-10-20T08:00:00",
    "endTime": "2025-10-20T16:00:00",
    "scheduleHours": 8
  },
  {
    "scheduleId": 6,
    "staffId": 2,
    "startTime": "2025-10-22T08:00:00",
    "endTime": "2025-10-22T16:00:00",
    "scheduleHours": 8
  }
]
```

**Error (400 Bad Request):**
```json
{
  "message": "Invalid date format. Use yyyy-MM-dd"
}
```

**Use Case:** Week navigation (Previous/Next week buttons)

---

### 4. Get Schedules with Custom Date Range
```http
GET /api/roster/staff/{staffId}?startDate={yyyy-MM-dd}&endDate={yyyy-MM-dd}
```

**Description**: Returns schedules for a specific staff member with optional date filtering.

**Auth Required**: Yes (Bearer Token)

**Access Control**: Workers can only view their own roster. Admins can view any roster.

**Query Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `startDate` | string (yyyy-MM-dd) | No | - | Filter schedules from this date onwards |
| `endDate` | string (yyyy-MM-dd) | No | - | Filter schedules up to this date (inclusive) |

**Request Examples:**
```bash
# Get all schedules for staff
GET /api/roster/staff/2
Authorization: Bearer {token}

# Get schedules for October 2025
GET /api/roster/staff/2?startDate=2025-10-01&endDate=2025-10-31
Authorization: Bearer {token}

# Get schedules from October 15 onwards
GET /api/roster/staff/2?startDate=2025-10-15
Authorization: Bearer {token}

# Get schedules up to October 31
GET /api/roster/staff/2?endDate=2025-10-31
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
[
  {
    "scheduleId": 7,
    "staffId": 2,
    "startTime": "2025-10-15T08:00:00",
    "endTime": "2025-10-15T16:00:00",
    "scheduleHours": 8
  },
  {
    "scheduleId": 8,
    "staffId": 2,
    "startTime": "2025-10-16T08:00:00",
    "endTime": "2025-10-16T16:00:00",
    "scheduleHours": 8
  }
]
```

**Use Case:** Custom date range filtering, calendar views

---

### 5. Get All Schedules
```http
GET /api/roster
```

**Description**: Returns all work schedules in the system. Optionally accepts custom SQL query.

**Auth Required**: No (Public)

**Query Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `query` | string | No | Custom SQL query |

**Request Examples:**
```bash
# Get all schedules
GET /api/roster

# Get schedules with custom query
GET /api/roster?query=SELECT * FROM WorkSchedule WHERE StaffId = 2
```

**Response (200 OK):**
```json
[
  {
    "scheduleId": 1,
    "staffId": 2,
    "startTime": "2025-10-16T08:00:00",
    "endTime": "2025-10-16T16:00:00",
    "scheduleHours": 8
  }
]
```

---

### 6. Get Schedule by ID
```http
GET /api/roster/{id}
```

**Description**: Returns a specific schedule by its ID.

**Auth Required**: No (Public)

**Request Example:**
```bash
GET /api/roster/1
```

**Response (200 OK):**
```json
{
  "scheduleId": 1,
  "staffId": 2,
  "startTime": "2025-10-16T08:00:00",
  "endTime": "2025-10-16T16:00:00",
  "scheduleHours": 8
}
```

**Error (404 Not Found):**
```json
{
  "message": "Schedule not found"
}
```

---

### 📝 **POST Operations (Admin Only)**

---

### 7. Assign Shift (Create Schedule)
```http
POST /api/roster/assign
```

**Description**: Assigns a new shift to a staff member. Automatically calculates hours, validates for overlapping shifts, and logs the action.

**Auth Required**: Yes (Admin only)

**PBIs Implemented:**
- **PBI 224**: Admin assigns shifts with date, start time, and end time
- **PBI 225**: Automatically calculates hours
- **PBI 226**: Prevents overlapping shifts
- **PBI 227**: Logs assignment actions

**Request Body:**
```json
{
  "StaffId": 2,
  "StartTime": "2025-10-16T08:00:00",
  "EndTime": "2025-10-16T16:00:00"
}
```

**Response (200 OK):**
```json
{
  "scheduleId": 10,
  "staffId": 2,
  "startTime": "2025-10-16T08:00:00",
  "endTime": "2025-10-16T16:00:00",
  "scheduleHours": 8
}
```

**Error (401 Unauthorized):**
```json
{
  "message": "Admin permission required to assign shifts"
}
```

**Error (400 Bad Request):**
```json
{
  "message": "End time must be after start time"
}
```

**Error (409 Conflict):**
```json
{
  "message": "Shift overlaps with existing schedule for this staff member on the same day"
}
```

---

### 8. Query Schedules (Custom SQL)
```http
POST /api/roster/query
```

**Description**: Execute custom SQL query for advanced schedule filtering.

**Auth Required**: No (Public)

**Request Body:**
```json
"SELECT * FROM WorkSchedule WHERE StaffId = 2 AND StartTime >= '2025-10-01'"
```

**Response (200 OK):**
```json
[
  {
    "scheduleId": 1,
    "staffId": 2,
    "startTime": "2025-10-15T08:00:00",
    "endTime": "2025-10-15T16:00:00",
    "scheduleHours": 8
  }
]
```

---

### 9. Create Schedule (Alternative Endpoint)
```http
POST /api/roster
```

**Description**: Alternative endpoint to create a schedule. Uses the same logic as `/assign`.

**Auth Required**: Yes (Admin only)

**Request Body:**
```json
{
  "staffId": 2,
  "startTime": "2025-10-16T08:00:00",
  "endTime": "2025-10-16T16:00:00"
}
```

**Response:** Same as `/assign` endpoint

---

### 🔧 **Utility Endpoints**

---

### 10. Validate Shift Overlap
```http
POST /api/roster/validate-overlap
```

**Description**: Checks if a shift would overlap with existing schedules without creating it. Useful for form validation.

**Auth Required**: Yes (Bearer Token)

**Request Body:**
```json
{
  "StaffId": 2,
  "StartTime": "2025-10-16T08:00:00",
  "EndTime": "2025-10-16T16:00:00",
  "ExcludeScheduleId": 5
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `StaffId` | integer | Yes | Staff member ID |
| `StartTime` | datetime | Yes | Shift start time |
| `EndTime` | datetime | Yes | Shift end time |
| `ExcludeScheduleId` | integer | No | Schedule ID to exclude from overlap check (for updates) |

**Response (200 OK):**
```json
{
  "hasOverlap": false,
  "calculatedHours": 8,
  "message": "No overlap detected"
}
```

**Response (200 OK - With Overlap):**
```json
{
  "hasOverlap": true,
  "calculatedHours": 8,
  "message": "Shift overlaps with existing schedule"
}
```

---

### 11. Calculate Hours
```http
POST /api/roster/calculate-hours
```

**Description**: Calculates hours between two timestamps.

**Auth Required**: No (Public)

**Request Body:**
```json
{
  "StartTime": "2025-10-16T08:00:00",
  "EndTime": "2025-10-16T16:00:00"
}
```

**Response (200 OK):**
```json
{
  "calculatedHours": 8,
  "startTime": "2025-10-16 08:00",
  "endTime": "2025-10-16 16:00"
}
```

**Error (400 Bad Request):**
```json
{
  "message": "End time must be after start time"
}
```

---

### ✏️ **PUT Operations (Admin Only)**

---

### 12. Update Schedule
```http
PUT /api/roster/{id}
```

**Description**: Updates an existing schedule with validation. Includes automatic hours calculation, overlap prevention, and audit logging.

**Auth Required**: Yes (Admin only)

**Request Body:**
```json
{
  "staffId": 2,
  "startTime": "2025-10-16T09:00:00",
  "endTime": "2025-10-16T17:00:00"
}
```

**Response (200 OK):**
```json
{
  "scheduleId": 1,
  "staffId": 2,
  "startTime": "2025-10-16T09:00:00",
  "endTime": "2025-10-16T17:00:00",
  "scheduleHours": 8
}
```

**Error (401 Unauthorized):**
```json
{
  "message": "Admin permission required to update schedules"
}
```

**Error (400 Bad Request):**
```json
{
  "message": "Schedule not found"
}
```

**Error (409 Conflict):**
```json
{
  "message": "Updated shift overlaps with existing schedule for this staff member on the same day"
}
```

---

### 🗑️ **DELETE Operations (Admin Only)**

---

### 13. Delete Schedule
```http
DELETE /api/roster/{id}
```

**Description**: Deletes a schedule. Logs the deletion action.

**Auth Required**: Yes (Admin only)

**Request Example:**
```bash
DELETE /api/roster/5
Authorization: Bearer {admin_token}
```

**Response (200 OK):**
```json
{
  "scheduleId": 5,
  "staffId": 2,
  "startTime": "2025-10-16T08:00:00",
  "endTime": "2025-10-16T16:00:00",
  "scheduleHours": 8
}
```

**Error (401 Unauthorized):**
```json
{
  "message": "Admin permission required to delete schedules"
}
```

**Error (400 Bad Request):**
```json
{
  "message": "Schedule not found"
}
```

---

## Business Logic

### Automatic Hours Calculation
- Hours are automatically calculated when creating or updating schedules
- Formula: `Math.Round((endTime - startTime).TotalHours)`
- Rounds to nearest whole hour

### Overlapping Shift Prevention
- System checks for overlapping shifts on the same day for the same staff member
- A shift overlaps if: `newStartTime < existingEndTime AND newEndTime > existingStartTime`
- Prevents double-booking workers

### Worker-Level Access Control
- **Workers**: Can ONLY view their own roster (`staffId` must match authenticated user)
- **Admins**: Can view and manage all rosters
- Unauthorized access attempts return 401 error

### Audit Logging
- All create, update, and delete operations are logged to the History table
- Logs include: action type, staff ID, schedule details, admin who performed action
- Logging failures don't block the main operation

---

## Security Features

### Authentication & Authorization
✅ **JWT Bearer Token** required for most endpoints
✅ **Role-Based Access Control** (Worker vs Admin)
✅ **Worker-Level Security** - Workers can only view their own roster
✅ **Admin Privileges** - Only admins can create/update/delete schedules

### Best Practices
- Always validate JWT token before processing requests
- Workers cannot access other workers' schedules
- Admins have full access for management purposes
- Sensitive operations (create/update/delete) require admin role

---

## Frontend Integration Guide

### Recommended Flow for Worker Dashboard

#### Step 1: Get Authenticated User
```javascript
// Extract staffId from JWT token after login
const token = localStorage.getItem('authToken');
const user = parseJWT(token);
const staffId = user.staffId; // e.g., 2
```

#### Step 2: Load Current Week (Dashboard)
```javascript
const loadCurrentWeek = async () => {
  const response = await axios.get(
    `/api/roster/staff/${staffId}/current-week`,
    { headers: { Authorization: `Bearer ${token}` } }
  );

  displayWeeklyRoster(response.data);
};
```

#### Step 3: Load Upcoming Weeks
```javascript
const loadUpcoming = async () => {
  const response = await axios.get(
    `/api/roster/staff/${staffId}/upcoming?weeks=4`,
    { headers: { Authorization: `Bearer ${token}` } }
  );

  displayUpcomingRoster(response.data);
};
```

#### Step 4: Week Navigation
```javascript
// Previous week
const goToPreviousWeek = async () => {
  const previousWeekDate = moment(currentDate).subtract(7, 'days').format('YYYY-MM-DD');

  const response = await axios.get(
    `/api/roster/staff/${staffId}/week?date=${previousWeekDate}`,
    { headers: { Authorization: `Bearer ${token}` } }
  );

  displayWeeklyRoster(response.data);
};

// Next week
const goToNextWeek = async () => {
  const nextWeekDate = moment(currentDate).add(7, 'days').format('YYYY-MM-DD');

  const response = await axios.get(
    `/api/roster/staff/${staffId}/week?date=${nextWeekDate}`,
    { headers: { Authorization: `Bearer ${token}` } }
  );

  displayWeeklyRoster(response.data);
};
```

#### Step 5: Real-Time Updates (Polling)
```javascript
// Poll for roster changes every 30 seconds
useEffect(() => {
  const interval = setInterval(() => {
    loadCurrentWeek(); // Refresh
  }, 30000);

  return () => clearInterval(interval);
}, []);
```

---

## React Component Example

```jsx
import { useState, useEffect } from 'react';
import axios from 'axios';

function WorkerRosterDashboard() {
  const [currentWeekSchedules, setCurrentWeekSchedules] = useState([]);
  const [upcomingSchedules, setUpcomingSchedules] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const token = localStorage.getItem('authToken');
  const staffId = getUserFromToken().staffId; // Your auth helper

  // Load current week on mount
  useEffect(() => {
    loadCurrentWeek();
    loadUpcoming();

    // Poll for updates every 30 seconds
    const interval = setInterval(loadCurrentWeek, 30000);
    return () => clearInterval(interval);
  }, []);

  const loadCurrentWeek = async () => {
    setLoading(true);
    setError(null);

    try {
      const response = await axios.get(
        `/api/roster/staff/${staffId}/current-week`,
        { headers: { Authorization: `Bearer ${token}` } }
      );

      setCurrentWeekSchedules(response.data);
    } catch (err) {
      setError(err.response?.data?.message || 'Error loading roster');
    } finally {
      setLoading(false);
    }
  };

  const loadUpcoming = async () => {
    try {
      const response = await axios.get(
        `/api/roster/staff/${staffId}/upcoming?weeks=4`,
        { headers: { Authorization: `Bearer ${token}` } }
      );

      setUpcomingSchedules(response.data);
    } catch (err) {
      console.error('Error loading upcoming schedules:', err);
    }
  };

  return (
    <div className="roster-dashboard">
      <h2>My Roster</h2>

      {loading && <p>Loading...</p>}
      {error && <p className="error">{error}</p>}

      <section className="current-week">
        <h3>This Week</h3>
        {currentWeekSchedules.map(schedule => (
          <div key={schedule.scheduleId} className="schedule-item">
            <p>Date: {new Date(schedule.startTime).toLocaleDateString()}</p>
            <p>Time: {new Date(schedule.startTime).toLocaleTimeString()} - {new Date(schedule.endTime).toLocaleTimeString()}</p>
            <p>Hours: {schedule.scheduleHours}</p>
          </div>
        ))}
      </section>

      <section className="upcoming">
        <h3>Upcoming (Next 4 Weeks)</h3>
        {upcomingSchedules.map(schedule => (
          <div key={schedule.scheduleId} className="schedule-item">
            <p>Date: {new Date(schedule.startTime).toLocaleDateString()}</p>
            <p>Time: {new Date(schedule.startTime).toLocaleTimeString()} - {new Date(schedule.endTime).toLocaleTimeString()}</p>
            <p>Hours: {schedule.scheduleHours}</p>
          </div>
        ))}
      </section>
    </div>
  );
}
```

---

## Testing

### Test with cURL

#### Get Current Week (Worker)
```bash
curl -X GET "http://localhost:5000/api/roster/staff/2/current-week" \
  -H "Authorization: Bearer YOUR_WORKER_TOKEN"
```

#### Get Upcoming Schedules
```bash
curl -X GET "http://localhost:5000/api/roster/staff/2/upcoming?weeks=4" \
  -H "Authorization: Bearer YOUR_WORKER_TOKEN"
```

#### Get Specific Week
```bash
curl -X GET "http://localhost:5000/api/roster/staff/2/week?date=2025-10-20" \
  -H "Authorization: Bearer YOUR_WORKER_TOKEN"
```

#### Assign Shift (Admin)
```bash
curl -X POST "http://localhost:5000/api/roster/assign" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "StaffId": 2,
    "StartTime": "2025-10-16T08:00:00",
    "EndTime": "2025-10-16T16:00:00"
  }'
```

---

## Quick Reference

| Use Case | Endpoint | Auth | Parameters |
|----------|----------|------|------------|
| View current week | `GET /staff/{id}/current-week` | Worker/Admin | None |
| View upcoming weeks | `GET /staff/{id}/upcoming` | Worker/Admin | `weeks` (optional) |
| Navigate to specific week | `GET /staff/{id}/week` | Worker/Admin | `date` (optional) |
| Custom date range | `GET /staff/{id}` | Worker/Admin | `startDate`, `endDate` (optional) |
| Assign shift | `POST /assign` | Admin | Body: StaffId, StartTime, EndTime |
| Update schedule | `PUT /{id}` | Admin | Body: staffId, startTime, endTime |
| Delete schedule | `DELETE /{id}` | Admin | None |
| Validate overlap | `POST /validate-overlap` | Worker/Admin | Body: StaffId, StartTime, EndTime |
| Calculate hours | `POST /calculate-hours` | Public | Body: StartTime, EndTime |

---

## Error Handling Best Practices

### Common Errors

| Status | Error | Solution |
|--------|-------|----------|
| 401 | "Workers can only view their own roster" | Ensure staffId matches authenticated user |
| 401 | "Authentication required" | Include valid JWT token in Authorization header |
| 401 | "Admin permission required" | Only admins can create/update/delete schedules |
| 400 | "Invalid date format" | Use yyyy-MM-dd format for dates |
| 400 | "End time must be after start time" | Validate dates before submitting |
| 409 | "Shift overlaps with existing schedule" | Check for overlaps using `/validate-overlap` endpoint |

### Error Handling in Frontend
```javascript
try {
  const response = await axios.get(`/api/roster/staff/${staffId}/current-week`, {
    headers: { Authorization: `Bearer ${token}` }
  });

  setSchedules(response.data);
} catch (error) {
  if (error.response?.status === 401) {
    // Redirect to login or show "Access Denied"
    handleUnauthorized();
  } else if (error.response?.status === 400) {
    // Show validation error
    setError(error.response.data.message);
  } else {
    // Show generic error
    setError('Failed to load roster. Please try again.');
  }
}
```

---

## Notes

### Date Format
- Always use **ISO 8601 format**: `yyyy-MM-dd` for dates, `yyyy-MM-ddTHH:mm:ss` for datetimes
- Example: `2025-10-16` or `2025-10-16T08:00:00`

### Week Calculation
- Weeks run from **Monday to Sunday**
- Any date passed to `/week` endpoint automatically calculates the Monday of that week
- Sunday belongs to the week starting on the previous Monday

### Performance Tips
- Use specific endpoints (`/current-week`, `/upcoming`) instead of custom queries when possible
- Implement client-side caching to reduce API calls
- Use polling interval of 30-60 seconds for real-time updates

### Security Reminders
- Never hardcode staffId in frontend - always use authenticated user's ID
- Always include JWT token in Authorization header
- Validate user permissions before showing admin features in UI

---

## Support

For issues or questions:
- Check this documentation first
- Review error messages carefully
- Contact the backend development team (Tan)

---

**Last Updated**: October 2025
**API Version**: 1.0
**Sprint**: Sprint 2
**Developer**: Tan
