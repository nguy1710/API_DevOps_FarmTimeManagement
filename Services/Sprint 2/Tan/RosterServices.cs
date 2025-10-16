using Microsoft.AspNetCore.Http;
using RestfulAPI_FarmTimeManagement.DataConnects;
using RestfulAPI_FarmTimeManagement.Models;
using RestfulAPI_FarmTimeManagement.Services.Sprint1.Tom;

namespace RestfulAPI_FarmTimeManagement.Services.Sprint_2.Tan
{
    public static class RosterServices
    {
        #region PBI 224: Admin Shift Assignment

        /// <summary>
        /// Assigns a shift to a staff member with validation
        /// </summary>
        public static async Task<WorkSchedule> AssignShift(int staffId, DateTime startTime, DateTime endTime, HttpContext httpContext)
        {
            // Validate admin permission
            var currentStaff = httpContext.Items["Staff"] as Staff;
            if (currentStaff?.Role != "Admin")
            {
                return new WorkSchedule { ScheduleId = -1, StaffId = -1, StartTime = DateTime.MinValue, EndTime = DateTime.MinValue, ScheduleHours = -1 };
            }

            // Validate end time is after start time
            if (endTime <= startTime)
            {
                throw new ArgumentException("End time must be after start time");
            }

            // Calculate hours automatically (PBI 225)
            var calculatedHours = CalculateHours(startTime, endTime);

            // Check for overlapping shifts (PBI 226)
            var hasOverlap = await CheckOverlappingShifts(staffId, startTime, endTime);
            if (hasOverlap)
            {
                throw new InvalidOperationException("Shift overlaps with existing schedule for this staff member on the same day");
            }

            // Create the shift
            var newShift = new WorkSchedule
            {
                StaffId = staffId,
                StartTime = startTime,
                EndTime = endTime,
                ScheduleHours = calculatedHours
            };

            var workScheduleConnects = new WorkScheduleConnects();
            var createdShift = await workScheduleConnects.CreateWorkSchedule(newShift);

            // Log the roster assignment (PBI 227)
            if (createdShift != null)
            {
                await LogRosterAction(currentStaff.StaffId, "create", createdShift, httpContext);
            }

            return createdShift ?? new WorkSchedule { ScheduleId = -1, StaffId = -1, StartTime = DateTime.MinValue, EndTime = DateTime.MinValue, ScheduleHours = -1 };
        }

        #endregion

        #region PBI 225: Automatic Hours Calculation

        /// <summary>
        /// Calculates total hours between start and end time
        /// </summary>
        public static int CalculateHours(DateTime startTime, DateTime endTime)
        {
            if (endTime <= startTime)
            {
                throw new ArgumentException("End time must be after start time");
            }

            var timeSpan = endTime - startTime;
            return (int)Math.Round(timeSpan.TotalHours);
        }

        #endregion

        #region PBI 226: Overlapping Shift Prevention

        /// <summary>
        /// Checks if a new shift overlaps with existing shifts for the same staff on the same day
        /// </summary>
        public static async Task<bool> CheckOverlappingShifts(int staffId, DateTime startTime, DateTime endTime, int? excludeScheduleId = null)
        {
            var workScheduleConnects = new WorkScheduleConnects();
            var dateOnly = startTime.Date;
            var nextDay = dateOnly.AddDays(1);

            // Query existing shifts for the same staff on the same day
            var query = $@"
                SELECT * FROM WorkSchedule
                WHERE StaffId = {staffId}
                AND StartTime >= '{dateOnly:yyyy-MM-dd}'
                AND StartTime < '{nextDay:yyyy-MM-dd}'";

            if (excludeScheduleId.HasValue)
            {
                query += $" AND ScheduleId != {excludeScheduleId.Value}";
            }

            var existingShifts = await workScheduleConnects.QueryWorkSchedule(query);

            // Check for any overlaps
            foreach (var shift in existingShifts)
            {
                // Check if new shift overlaps with existing shift
                if (startTime < shift.EndTime && endTime > shift.StartTime)
                {
                    return true; // Overlap detected
                }
            }

            return false; // No overlap
        }

        #endregion

        #region PBI 227: Roster Assignment Logging

        /// <summary>
        /// Logs roster assignment actions
        /// </summary>
        private static async Task LogRosterAction(int adminId, string actionType, WorkSchedule schedule, HttpContext httpContext)
        {
            try
            {
                var logMessage = $"Roster {actionType}: Staff {schedule.StaffId}, Schedule {schedule.ScheduleId}, " +
                               $"Start: {schedule.StartTime:yyyy-MM-dd HH:mm}, End: {schedule.EndTime:yyyy-MM-dd HH:mm}, " +
                               $"Hours: {schedule.ScheduleHours}";

                await HistoryServices.CreateHistory(
                    $"ROSTER_{actionType.ToUpper()}",
                    "SUCCESS",
                    logMessage,
                    httpContext
                );
            }
            catch (Exception ex)
            {
                // Log error but don't fail the main operation
                Console.WriteLine($"Failed to log roster action: {ex.Message}");
            }
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// Gets all schedules with optional filtering
        /// </summary>
        public static async Task<List<WorkSchedule>> GetAllSchedules(string? query = null)
        {
            var workScheduleConnects = new WorkScheduleConnects();
            return await workScheduleConnects.QueryWorkSchedule(query ?? "SELECT * FROM WorkSchedule");
        }

        /// <summary>
        /// Gets a specific schedule by ID
        /// </summary>
        public static async Task<WorkSchedule?> GetScheduleById(int scheduleId)
        {
            var workScheduleConnects = new WorkScheduleConnects();
            var schedules = await workScheduleConnects.QueryWorkSchedule($"SELECT * FROM WorkSchedule WHERE ScheduleId = {scheduleId}");
            return schedules.FirstOrDefault();
        }

        /// <summary>
        /// Gets schedules for a specific staff member
        /// </summary>
        public static async Task<List<WorkSchedule>> GetSchedulesByStaffId(int staffId)
        {
            var workScheduleConnects = new WorkScheduleConnects();
            return await workScheduleConnects.QueryWorkSchedule($"SELECT * FROM WorkSchedule WHERE StaffId = {staffId} ORDER BY StartTime");
        }

        /// <summary>
        /// Gets schedules for a specific staff member with date range filtering
        /// PBI: Worker Roster Viewing - Filter by current and upcoming weeks
        /// </summary>
        public static async Task<List<WorkSchedule>> GetSchedulesByStaffIdWithDateRange(int staffId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var workScheduleConnects = new WorkScheduleConnects();

            // Build query with optional date filtering
            string query = $"SELECT * FROM WorkSchedule WHERE StaffId = {staffId}";

            if (startDate.HasValue)
            {
                query += $" AND StartTime >= '{startDate.Value:yyyy-MM-dd}'";
            }

            if (endDate.HasValue)
            {
                // Add one day to include the entire end date
                var endDateInclusive = endDate.Value.AddDays(1);
                query += $" AND StartTime < '{endDateInclusive:yyyy-MM-dd}'";
            }

            query += " ORDER BY StartTime";

            return await workScheduleConnects.QueryWorkSchedule(query);
        }

        /// <summary>
        /// Gets schedules for the current week (Monday to Sunday) for a specific staff member
        /// </summary>
        public static async Task<List<WorkSchedule>> GetCurrentWeekSchedules(int staffId)
        {
            var today = DateTime.Today;
            var currentWeekMonday = GetMondayOfWeek(today);
            var currentWeekSunday = currentWeekMonday.AddDays(6);

            return await GetSchedulesByStaffIdWithDateRange(staffId, currentWeekMonday, currentWeekSunday);
        }

        /// <summary>
        /// Gets schedules for upcoming weeks (from today onwards) for a specific staff member
        /// </summary>
        public static async Task<List<WorkSchedule>> GetUpcomingSchedules(int staffId, int weeksAhead = 4)
        {
            var today = DateTime.Today;
            var endDate = today.AddDays(7 * weeksAhead);

            return await GetSchedulesByStaffIdWithDateRange(staffId, today, endDate);
        }

        /// <summary>
        /// Gets schedules for a specific week (Monday to Sunday) for a staff member
        /// </summary>
        public static async Task<List<WorkSchedule>> GetWeekSchedules(int staffId, DateTime weekDate)
        {
            var weekMonday = GetMondayOfWeek(weekDate);
            var weekSunday = weekMonday.AddDays(6);

            return await GetSchedulesByStaffIdWithDateRange(staffId, weekMonday, weekSunday);
        }

        /// <summary>
        /// Helper method to get the Monday of a given week
        /// </summary>
        private static DateTime GetMondayOfWeek(DateTime date)
        {
            var dayOfWeek = date.DayOfWeek;
            var daysToSubtract = dayOfWeek == DayOfWeek.Sunday ? 6 : (int)dayOfWeek - 1;
            return date.AddDays(-daysToSubtract).Date;
        }

        /// <summary>
        /// Validates that a worker can only access their own roster
        /// Returns the staffId if authorized, throws exception otherwise
        /// </summary>
        public static int ValidateWorkerAccess(int requestedStaffId, HttpContext httpContext)
        {
            var currentStaff = httpContext.Items["Staff"] as Staff;

            if (currentStaff == null)
            {
                throw new UnauthorizedAccessException("Authentication required");
            }

            // Admins can view any roster
            if (currentStaff.Role == "Admin")
            {
                return requestedStaffId;
            }

            // Workers can only view their own roster
            if (currentStaff.StaffId != requestedStaffId)
            {
                throw new UnauthorizedAccessException("Workers can only view their own roster");
            }

            return requestedStaffId;
        }

        /// <summary>
        /// Updates an existing schedule with validation
        /// </summary>
        public static async Task<WorkSchedule> UpdateSchedule(int scheduleId, WorkSchedule updatedSchedule, HttpContext httpContext)
        {
            // Validate admin permission
            var currentStaff = httpContext.Items["Staff"] as Staff;
            if (currentStaff?.Role != "Admin")
            {
                return new WorkSchedule { ScheduleId = -1, StaffId = -1, StartTime = DateTime.MinValue, EndTime = DateTime.MinValue, ScheduleHours = -1 };
            }

            // Validate end time is after start time
            if (updatedSchedule.EndTime <= updatedSchedule.StartTime)
            {
                throw new ArgumentException("End time must be after start time");
            }

            // Calculate hours automatically
            updatedSchedule.ScheduleHours = CalculateHours(updatedSchedule.StartTime, updatedSchedule.EndTime);

            // Check for overlapping shifts (excluding current schedule)
            var hasOverlap = await CheckOverlappingShifts(updatedSchedule.StaffId, updatedSchedule.StartTime, updatedSchedule.EndTime, scheduleId);
            if (hasOverlap)
            {
                throw new InvalidOperationException("Updated shift overlaps with existing schedule for this staff member on the same day");
            }

            var workScheduleConnects = new WorkScheduleConnects();
            var result = await workScheduleConnects.UpdateWorkSchedule(scheduleId, updatedSchedule);

            // Log the update
            if (result != null)
            {
                await LogRosterAction(currentStaff.StaffId, "update", result, httpContext);
            }

            return result ?? new WorkSchedule { ScheduleId = -1, StaffId = -1, StartTime = DateTime.MinValue, EndTime = DateTime.MinValue, ScheduleHours = -1 };
        }

        /// <summary>
        /// Deletes a schedule
        /// </summary>
        public static async Task<WorkSchedule> DeleteSchedule(int scheduleId, HttpContext httpContext)
        {
            // Validate admin permission
            var currentStaff = httpContext.Items["Staff"] as Staff;
            if (currentStaff?.Role != "Admin")
            {
                return new WorkSchedule { ScheduleId = -1, StaffId = -1, StartTime = DateTime.MinValue, EndTime = DateTime.MinValue, ScheduleHours = -1 };
            }

            // Get the schedule before deletion for logging
            var scheduleToDelete = await GetScheduleById(scheduleId);
            if (scheduleToDelete == null)
            {
                throw new ArgumentException("Schedule not found");
            }

            var workScheduleConnects = new WorkScheduleConnects();
            var result = await workScheduleConnects.DeleteWorkSchedule(scheduleId);

            // Log the deletion
            if (result != null)
            {
                await LogRosterAction(currentStaff.StaffId, "delete", result, httpContext);
            }

            return result ?? new WorkSchedule { ScheduleId = -1, StaffId = -1, StartTime = DateTime.MinValue, EndTime = DateTime.MinValue, ScheduleHours = -1 };
        }

        #endregion
    }
}