using Microsoft.AspNetCore.Http;
using RestfulAPI_FarmTimeManagement.Models;
using RestfulAPI_FarmTimeManagement.Services.Sprint_2.Tan;

namespace RestfulAPI_FarmTimeManagement.Services.Sprint_2.Tim
{
    public static class RosterValidationService
    {
        // Configuration constants for flexibility
        private const int EARLY_CLOCK_IN_MINUTES = 15;
        private const int LATE_CLOCK_IN_MINUTES = 30;
        private const int EARLY_CLOCK_OUT_MINUTES = 0;
        private const int LATE_CLOCK_OUT_MINUTES = 15;

        /// <summary>
        /// Validates if staff can clock in at the current time based on their roster
        /// </summary>
        /// <param name="staffId">Staff ID attempting to clock in</param>
        /// <param name="clockInTime">Time of clock in attempt (optional, defaults to now)</param>
        /// <returns>Validation result with success status and message</returns>
        public static async Task<RosterValidationResult> ValidateClockIn(int staffId, DateTime? clockInTime = null)
        {
            try
            {
                var attemptTime = clockInTime ?? DateTime.Now;
                var targetDate = attemptTime.Date;

                // Get staff roster for the target date
                var roster = await GetStaffRosterForDate(staffId, targetDate);

                if (roster == null)
                {
                    return new RosterValidationResult
                    {
                        IsValid = false,
                        Message = $"No roster assignment found for staff ID {staffId} on {targetDate:yyyy-MM-dd}",
                        ValidationCode = "NO_ROSTER"
                    };
                }

                // Calculate allowed clock-in window
                var earliestClockIn = roster.StartTime.AddMinutes(-EARLY_CLOCK_IN_MINUTES);
                var latestClockIn = roster.StartTime.AddMinutes(LATE_CLOCK_IN_MINUTES);

                // Handle cross-day shifts (e.g., night shifts)
                if (roster.EndTime < roster.StartTime)
                {
                    // Night shift scenario
                    if (attemptTime.TimeOfDay >= earliestClockIn.TimeOfDay ||
                        attemptTime.TimeOfDay <= latestClockIn.TimeOfDay)
                    {
                        return CreateSuccessResult(roster, "clock-in", attemptTime);
                    }
                }
                else
                {
                    // Regular day shift
                    if (attemptTime.TimeOfDay >= earliestClockIn.TimeOfDay &&
                        attemptTime.TimeOfDay <= latestClockIn.TimeOfDay)
                    {
                        return CreateSuccessResult(roster, "clock-in", attemptTime);
                    }
                }

                return new RosterValidationResult
                {
                    IsValid = false,
                    Message = $"Clock-in time {attemptTime:HH:mm} is outside allowed window ({earliestClockIn:HH:mm} - {latestClockIn:HH:mm})",
                    ValidationCode = "OUTSIDE_WINDOW",
                    RosterInfo = FormatRosterInfo(roster)
                };
            }
            catch (Exception ex)
            {
                return new RosterValidationResult
                {
                    IsValid = false,
                    Message = $"Validation error: {ex.Message}",
                    ValidationCode = "SYSTEM_ERROR"
                };
            }
        }

        /// <summary>
        /// Validates if staff can clock out at the current time based on their roster
        /// </summary>
        /// <param name="staffId">Staff ID attempting to clock out</param>
        /// <param name="clockOutTime">Time of clock out attempt (optional, defaults to now)</param>
        /// <returns>Validation result with success status and message</returns>
        public static async Task<RosterValidationResult> ValidateClockOut(int staffId, DateTime? clockOutTime = null)
        {
            try
            {
                var attemptTime = clockOutTime ?? DateTime.Now;
                var targetDate = attemptTime.Date;

                // For clock-out, we might need to check both current day and previous day roster
                var roster = await GetStaffRosterForDate(staffId, targetDate) ??
                            await GetStaffRosterForDate(staffId, targetDate.AddDays(-1));

                if (roster == null)
                {
                    return new RosterValidationResult
                    {
                        IsValid = false,
                        Message = $"No roster assignment found for staff ID {staffId} around {targetDate:yyyy-MM-dd}",
                        ValidationCode = "NO_ROSTER"
                    };
                }

                // Calculate allowed clock-out window
                var earliestClockOut = roster.EndTime.AddMinutes(-EARLY_CLOCK_OUT_MINUTES);
                var latestClockOut = roster.EndTime.AddMinutes(LATE_CLOCK_OUT_MINUTES);

                // Handle cross-day shifts
                if (roster.EndTime < roster.StartTime)
                {
                    // Night shift - end time is next day
                    var nextDayEndTime = targetDate.AddDays(1).Add(roster.EndTime.TimeOfDay);
                    var earliestNextDay = nextDayEndTime.AddMinutes(-EARLY_CLOCK_OUT_MINUTES);
                    var latestNextDay = nextDayEndTime.AddMinutes(LATE_CLOCK_OUT_MINUTES);

                    if (attemptTime >= earliestNextDay && attemptTime <= latestNextDay)
                    {
                        return CreateSuccessResult(roster, "clock-out", attemptTime);
                    }
                }
                else
                {
                    // Regular day shift
                    if (attemptTime.TimeOfDay >= earliestClockOut.TimeOfDay &&
                        attemptTime.TimeOfDay <= latestClockOut.TimeOfDay)
                    {
                        return CreateSuccessResult(roster, "clock-out", attemptTime);
                    }
                }

                return new RosterValidationResult
                {
                    IsValid = false,
                    Message = $"Clock-out time {attemptTime:HH:mm} is outside allowed window ({earliestClockOut:HH:mm} - {latestClockOut:HH:mm})",
                    ValidationCode = "OUTSIDE_WINDOW",
                    RosterInfo = FormatRosterInfo(roster)
                };
            }
            catch (Exception ex)
            {
                return new RosterValidationResult
                {
                    IsValid = false,
                    Message = $"Validation error: {ex.Message}",
                    ValidationCode = "SYSTEM_ERROR"
                };
            }
        }

        /// <summary>
        /// Gets roster information for a specific staff member on a specific date
        /// </summary>
        private static async Task<WorkSchedule?> GetStaffRosterForDate(int staffId, DateTime date)
        {
            try
            {
                // Query WorkSchedule directly since RosterServices doesn't have GetWorkSchedules
                var workScheduleConnects = new DataConnects.WorkScheduleConnects();
                var dateOnly = date.Date;
                var nextDay = dateOnly.AddDays(1);

                var query = $@"
                    SELECT * FROM WorkSchedule
                    WHERE StaffId = {staffId}
                    AND StartTime >= '{dateOnly:yyyy-MM-dd}'
                    AND StartTime < '{nextDay:yyyy-MM-dd}'";

                var schedules = await workScheduleConnects.QueryWorkSchedule(query);
                return schedules.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a successful validation result
        /// </summary>
        private static RosterValidationResult CreateSuccessResult(WorkSchedule roster, string action, DateTime attemptTime)
        {
            return new RosterValidationResult
            {
                IsValid = true,
                Message = $"Valid {action} time {attemptTime:HH:mm} for scheduled shift {roster.StartTime:HH:mm}-{roster.EndTime:HH:mm}",
                ValidationCode = "SUCCESS",
                RosterInfo = FormatRosterInfo(roster)
            };
        }

        /// <summary>
        /// Formats roster information for user display
        /// </summary>
        private static string FormatRosterInfo(WorkSchedule roster)
        {
            return $"Scheduled: {roster.StartTime:yyyy-MM-dd HH:mm}-{roster.EndTime:HH:mm} ({roster.ScheduleHours}h)";
        }

        /// <summary>
        /// Checks if a staff member has any roster assignment for today
        /// </summary>
        public static async Task<bool> HasRosterToday(int staffId)
        {
            var roster = await GetStaffRosterForDate(staffId, DateTime.Today);
            return roster != null;
        }

        /// <summary>
        /// Gets the next scheduled shift for a staff member
        /// </summary>
        public static async Task<WorkSchedule?> GetNextScheduledShift(int staffId)
        {
            try
            {
                var workScheduleConnects = new DataConnects.WorkScheduleConnects();
                var today = DateTime.Today;

                var query = $@"
                    SELECT TOP 1 * FROM WorkSchedule
                    WHERE StaffId = {staffId}
                    AND StartTime >= '{today:yyyy-MM-dd}'
                    ORDER BY StartTime ASC";

                var schedules = await workScheduleConnects.QueryWorkSchedule(query);
                return schedules.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Result class for roster validation operations
    /// </summary>
    public class RosterValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ValidationCode { get; set; } = string.Empty;
        public string? RosterInfo { get; set; }
    }
}