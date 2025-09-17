using RestfulAPI_FarmTimeManagement.Models;

namespace RestfulAPI_FarmTimeManagement.Services.Sprint_2.Tim
{
    /// <summary>
    /// Tim's Message Formatter Service - PBI 8.3.2 & 8.4.2 Implementation
    /// Provides standardized, user-friendly messages for clock-in/out operations
    /// </summary>
    public static class MessageFormatterService
    {
        /// <summary>
        /// Standardized response format for Tim's enhanced endpoints
        /// </summary>
        public class FormattedResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public string? DetailedMessage { get; set; }
            public int? EventId { get; set; }
            public DateTime Timestamp { get; set; } = DateTime.Now;
            public string? ValidationCode { get; set; }
            public object? AdditionalData { get; set; }
        }

        /// <summary>
        /// PBI 8.3.2: Format clock-in success message
        /// Creates the required "Clock-in recorded at HH:MM" format
        /// </summary>
        /// <param name="eventResult">The created event from EventServices</param>
        /// <param name="staffName">Optional staff name for personalization</param>
        /// <returns>Formatted success response</returns>
        public static FormattedResponse FormatClockInSuccess(Event eventResult, string? staffName = null)
        {
            var timeString = eventResult.Timestamp.ToString("HH:mm");
            var dateString = eventResult.Timestamp.ToString("yyyy-MM-dd");

            var mainMessage = $"Clock-in recorded at {timeString}";
            var detailedMessage = string.IsNullOrEmpty(staffName)
                ? $"Clock-in successfully recorded on {dateString} at {timeString}"
                : $"Clock-in for {staffName} successfully recorded on {dateString} at {timeString}";

            return new FormattedResponse
            {
                Success = true,
                Message = mainMessage,
                DetailedMessage = detailedMessage,
                EventId = eventResult.EventId,
                Timestamp = eventResult.Timestamp,
                ValidationCode = "CLOCK_IN_SUCCESS"
            };
        }

        /// <summary>
        /// PBI 8.4.2: Format clock-out success message
        /// Creates the required "Clock-out recorded at HH:MM" format
        /// </summary>
        /// <param name="eventResult">The created event from EventServices</param>
        /// <param name="staffName">Optional staff name for personalization</param>
        /// <returns>Formatted success response</returns>
        public static FormattedResponse FormatClockOutSuccess(Event eventResult, string? staffName = null)
        {
            var timeString = eventResult.Timestamp.ToString("HH:mm");
            var dateString = eventResult.Timestamp.ToString("yyyy-MM-dd");

            var mainMessage = $"Clock-out recorded at {timeString}";
            var detailedMessage = string.IsNullOrEmpty(staffName)
                ? $"Clock-out successfully recorded on {dateString} at {timeString}"
                : $"Clock-out for {staffName} successfully recorded on {dateString} at {timeString}";

            return new FormattedResponse
            {
                Success = true,
                Message = mainMessage,
                DetailedMessage = detailedMessage,
                EventId = eventResult.EventId,
                Timestamp = eventResult.Timestamp,
                ValidationCode = "CLOCK_OUT_SUCCESS"
            };
        }

        /// <summary>
        /// Format validation failure messages with actionable guidance
        /// </summary>
        public static FormattedResponse FormatValidationError(string errorMessage, string validationCode, string? guidance = null)
        {
            return new FormattedResponse
            {
                Success = false,
                Message = errorMessage,
                DetailedMessage = guidance ?? "Please check your roster schedule or contact your supervisor.",
                ValidationCode = validationCode,
                Timestamp = DateTime.Now
            };
        }

        /// <summary>
        /// Format admin override success messages
        /// </summary>
        public static FormattedResponse FormatAdminOverrideSuccess(Event eventResult, string action, string reason, string adminName)
        {
            var timeString = eventResult.Timestamp.ToString("HH:mm");
            var mainMessage = $"{action} recorded at {timeString} (Admin Override)";
            var detailedMessage = $"Admin {adminName} authorized {action.ToLower()} override. Reason: {reason}";

            return new FormattedResponse
            {
                Success = true,
                Message = mainMessage,
                DetailedMessage = detailedMessage,
                EventId = eventResult.EventId,
                Timestamp = eventResult.Timestamp,
                ValidationCode = "ADMIN_OVERRIDE_SUCCESS",
                AdditionalData = new { AdminOverride = true, Reason = reason, AdminName = adminName }
            };
        }

        /// <summary>
        /// Format system error messages with helpful context
        /// </summary>
        public static FormattedResponse FormatSystemError(string errorMessage, string? context = null)
        {
            return new FormattedResponse
            {
                Success = false,
                Message = "System error occurred during operation",
                DetailedMessage = $"Error: {errorMessage}. {context ?? "Please try again or contact support."}",
                ValidationCode = "SYSTEM_ERROR",
                Timestamp = DateTime.Now
            };
        }

        /// <summary>
        /// Format roster status information for user display
        /// </summary>
        public static object FormatRosterStatus(int staffId, string staffName, bool hasRosterToday, WorkSchedule? nextShift)
        {
            var baseInfo = new
            {
                staffId = staffId,
                staffName = staffName,
                hasRosterToday = hasRosterToday,
                message = hasRosterToday
                    ? "You have a roster assignment today"
                    : "No roster assignment found for today",
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            if (nextShift != null)
            {
                return new
                {
                    staffId = baseInfo.staffId,
                    staffName = baseInfo.staffName,
                    hasRosterToday = baseInfo.hasRosterToday,
                    message = baseInfo.message,
                    nextShift = new
                    {
                        date = nextShift.StartTime.ToString("yyyy-MM-dd"),
                        startTime = nextShift.StartTime.ToString("HH:mm"),
                        endTime = nextShift.EndTime.ToString("HH:mm"),
                        totalHours = nextShift.ScheduleHours,
                        formattedSchedule = $"{nextShift.StartTime:yyyy-MM-dd HH:mm} - {nextShift.EndTime:HH:mm} ({nextShift.ScheduleHours}h)"
                    },
                    guidance = hasRosterToday
                        ? "You can clock in during your scheduled hours"
                        : "Please wait for your next scheduled shift",
                    timestamp = baseInfo.timestamp
                };
            }

            return new
            {
                staffId = baseInfo.staffId,
                staffName = baseInfo.staffName,
                hasRosterToday = baseInfo.hasRosterToday,
                message = baseInfo.message,
                nextShift = (object?)null,
                guidance = "No upcoming shifts found. Please contact your supervisor.",
                timestamp = baseInfo.timestamp
            };
        }

        /// <summary>
        /// Format dry-run validation results with clear guidance
        /// </summary>
        public static object FormatValidationResult(bool isValid, string message, string validationCode, string action, string? rosterInfo = null)
        {
            var guidance = isValid
                ? $"You can proceed with {action}"
                : GetValidationGuidance(validationCode);

            return new
            {
                isValid = isValid,
                message = message,
                validationCode = validationCode,
                action = action,
                rosterInfo = rosterInfo,
                guidance = guidance,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
        }

        /// <summary>
        /// Provides specific guidance based on validation failure codes
        /// </summary>
        private static string GetValidationGuidance(string validationCode)
        {
            return validationCode switch
            {
                "NO_ROSTER" => "Contact your supervisor to get a roster assignment for today.",
                "OUTSIDE_WINDOW" => "Please wait for your scheduled shift time or contact your supervisor for override.",
                "DUPLICATE_ENTRY" => "You have already clocked in/out recently. Please wait before trying again.",
                "INVALID_SEQUENCE" => "Please ensure you clock in before clocking out.",
                "INSUFFICIENT_PRIVILEGES" => "Only administrators can perform this action.",
                _ => "Please contact your supervisor for assistance."
            };
        }

        /// <summary>
        /// Format time-based messages with user's local timezone consideration
        /// </summary>
        public static string FormatTimeMessage(DateTime timestamp, string action)
        {
            var timeString = timestamp.ToString("HH:mm");
            var dateString = timestamp.ToString("yyyy-MM-dd");

            return $"{action} recorded at {timeString} on {dateString}";
        }

        /// <summary>
        /// Create comprehensive operation summary for logging
        /// </summary>
        public static string CreateOperationSummary(string operation, int staffId, int? deviceId, bool success, string? reason = null)
        {
            var parts = new List<string>
            {
                $"Operation: {operation}",
                $"Staff ID: {staffId}",
                $"Status: {(success ? "Success" : "Failed")}"
            };

            if (deviceId.HasValue)
                parts.Add($"Device ID: {deviceId}");

            if (!string.IsNullOrEmpty(reason))
                parts.Add($"Reason: {reason}");

            parts.Add($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            return string.Join(" | ", parts);
        }
    }
}