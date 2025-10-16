using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestfulAPI_FarmTimeManagement.Models;
using RestfulAPI_FarmTimeManagement.Services.Sprint_2.Tan;

namespace RestfulAPI_FarmTimeManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RosterController : ControllerBase
    {
        #region GET Operations

        /// <summary>
        /// GET: api/roster - Gets all schedules or with custom query
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? query)
        {
            try
            {
                var schedules = await RosterServices.GetAllSchedules(query);
                return new OkObjectResult(JsonConvert.SerializeObject(schedules));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/roster/query - Custom SQL query for schedules
        /// </summary>
        [HttpPost("query")]
        public async Task<IActionResult> QueryWithBody([FromBody] string query)
        {
            try
            {
                var schedules = await RosterServices.GetAllSchedules(query);
                return new OkObjectResult(JsonConvert.SerializeObject(schedules));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/roster/{id} - Gets a specific schedule by ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var schedule = await RosterServices.GetScheduleById(id);
                if (schedule == null)
                {
                    return NotFound(new { message = "Schedule not found" });
                }
                return new OkObjectResult(JsonConvert.SerializeObject(schedule));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/roster/staff/{staffId} - Gets all schedules for a specific staff member
        /// Query parameters: startDate (yyyy-MM-dd), endDate (yyyy-MM-dd)
        /// Workers can only view their own roster, Admins can view any roster
        /// </summary>
        [Authorize]
        [HttpGet("staff/{staffId:int}")]
        public async Task<IActionResult> GetByStaffId(int staffId, [FromQuery] string? startDate = null, [FromQuery] string? endDate = null)
        {
            try
            {
                // Validate worker access (workers can only view their own roster)
                RosterServices.ValidateWorkerAccess(staffId, HttpContext);

                // Parse optional date parameters
                DateTime? parsedStartDate = null;
                DateTime? parsedEndDate = null;

                if (!string.IsNullOrEmpty(startDate))
                {
                    if (!DateTime.TryParse(startDate, out var tempStartDate))
                    {
                        return BadRequest(new { message = "Invalid startDate format. Use yyyy-MM-dd" });
                    }
                    parsedStartDate = tempStartDate;
                }

                if (!string.IsNullOrEmpty(endDate))
                {
                    if (!DateTime.TryParse(endDate, out var tempEndDate))
                    {
                        return BadRequest(new { message = "Invalid endDate format. Use yyyy-MM-dd" });
                    }
                    parsedEndDate = tempEndDate;
                }

                // Get schedules with optional date filtering
                var schedules = await RosterServices.GetSchedulesByStaffIdWithDateRange(staffId, parsedStartDate, parsedEndDate);
                return new OkObjectResult(JsonConvert.SerializeObject(schedules));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/roster/staff/{staffId}/current-week - Gets current week schedule for a staff member
        /// Returns Monday to Sunday of the current week
        /// </summary>
        [Authorize]
        [HttpGet("staff/{staffId:int}/current-week")]
        public async Task<IActionResult> GetCurrentWeek(int staffId)
        {
            try
            {
                // Validate worker access
                RosterServices.ValidateWorkerAccess(staffId, HttpContext);

                var schedules = await RosterServices.GetCurrentWeekSchedules(staffId);
                return new OkObjectResult(JsonConvert.SerializeObject(schedules));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/roster/staff/{staffId}/upcoming - Gets upcoming schedules for a staff member
        /// Query parameter: weeks (default: 4) - number of weeks ahead to retrieve
        /// </summary>
        [Authorize]
        [HttpGet("staff/{staffId:int}/upcoming")]
        public async Task<IActionResult> GetUpcoming(int staffId, [FromQuery] int weeks = 4)
        {
            try
            {
                // Validate worker access
                RosterServices.ValidateWorkerAccess(staffId, HttpContext);

                if (weeks < 1 || weeks > 52)
                {
                    return BadRequest(new { message = "Weeks parameter must be between 1 and 52" });
                }

                var schedules = await RosterServices.GetUpcomingSchedules(staffId, weeks);
                return new OkObjectResult(JsonConvert.SerializeObject(schedules));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/roster/staff/{staffId}/week - Gets schedules for a specific week
        /// Query parameter: date (yyyy-MM-dd) - any date in the desired week
        /// Returns Monday to Sunday of that week
        /// </summary>
        [Authorize]
        [HttpGet("staff/{staffId:int}/week")]
        public async Task<IActionResult> GetWeek(int staffId, [FromQuery] string? date = null)
        {
            try
            {
                // Validate worker access
                RosterServices.ValidateWorkerAccess(staffId, HttpContext);

                // Parse date parameter (defaults to today if not provided)
                DateTime weekDate = DateTime.Today;
                if (!string.IsNullOrEmpty(date))
                {
                    if (!DateTime.TryParse(date, out weekDate))
                    {
                        return BadRequest(new { message = "Invalid date format. Use yyyy-MM-dd" });
                    }
                }

                var schedules = await RosterServices.GetWeekSchedules(staffId, weekDate);
                return new OkObjectResult(JsonConvert.SerializeObject(schedules));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region POST Operations

        /// <summary>
        /// POST: api/roster/assign - Assigns a new shift to a staff member
        /// PBI 224: Admin assigns shifts with date, start time, and end time
        /// PBI 225: Automatically calculates hours
        /// PBI 226: Prevents overlapping shifts
        /// PBI 227: Logs assignment actions
        /// </summary>
        [Authorize]
        [HttpPost("assign")]
        public async Task<IActionResult> AssignShift([FromBody] object body)
        {
            try
            {
                var requestData = JsonConvert.DeserializeObject<Dictionary<string, object>>(body.ToString());

                if (!requestData.ContainsKey("StaffId") || !requestData.ContainsKey("StartTime") || !requestData.ContainsKey("EndTime"))
                {
                    return BadRequest(new { message = "StaffId, StartTime, and EndTime are required" });
                }

                var staffId = Convert.ToInt32(requestData["StaffId"]);
                var startTime = Convert.ToDateTime(requestData["StartTime"]);
                var endTime = Convert.ToDateTime(requestData["EndTime"]);

                var assignedShift = await RosterServices.AssignShift(staffId, startTime, endTime, HttpContext);

                if (assignedShift.ScheduleId == -1)
                {
                    return Unauthorized(new { message = "Admin permission required to assign shifts" });
                }

                return new OkObjectResult(JsonConvert.SerializeObject(assignedShift));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/roster - Creates a new schedule (alternative endpoint)
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] object body)
        {
            try
            {
                var schedule = JsonConvert.DeserializeObject<WorkSchedule>(body.ToString());

                var assignedShift = await RosterServices.AssignShift(
                    schedule.StaffId,
                    schedule.StartTime,
                    schedule.EndTime,
                    HttpContext);

                if (assignedShift.ScheduleId == -1)
                {
                    return Unauthorized(new { message = "Admin permission required to create schedules" });
                }

                return new OkObjectResult(JsonConvert.SerializeObject(assignedShift));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        #endregion

        #region PUT Operations

        /// <summary>
        /// PUT: api/roster/{id} - Updates an existing schedule
        /// Includes all validations and logging
        /// </summary>
        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] object body)
        {
            try
            {
                var schedule = JsonConvert.DeserializeObject<WorkSchedule>(body.ToString());

                var updatedSchedule = await RosterServices.UpdateSchedule(id, schedule, HttpContext);

                if (updatedSchedule.ScheduleId == -1)
                {
                    return Unauthorized(new { message = "Admin permission required to update schedules" });
                }

                return new OkObjectResult(JsonConvert.SerializeObject(updatedSchedule));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        #endregion

        #region DELETE Operations

        /// <summary>
        /// DELETE: api/roster/{id} - Deletes a schedule
        /// </summary>
        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deletedSchedule = await RosterServices.DeleteSchedule(id, HttpContext);

                if (deletedSchedule.ScheduleId == -1)
                {
                    return Unauthorized(new { message = "Admin permission required to delete schedules" });
                }

                return new OkObjectResult(JsonConvert.SerializeObject(deletedSchedule));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        #endregion

        #region Utility Endpoints

        /// <summary>
        /// POST: api/roster/validate-overlap - Checks for shift overlaps without creating
        /// </summary>
        [Authorize]
        [HttpPost("validate-overlap")]
        public async Task<IActionResult> ValidateOverlap([FromBody] object body)
        {
            try
            {
                var requestData = JsonConvert.DeserializeObject<Dictionary<string, object>>(body.ToString());

                if (!requestData.ContainsKey("StaffId") || !requestData.ContainsKey("StartTime") || !requestData.ContainsKey("EndTime"))
                {
                    return BadRequest(new { message = "StaffId, StartTime, and EndTime are required" });
                }

                var staffId = Convert.ToInt32(requestData["StaffId"]);
                var startTime = Convert.ToDateTime(requestData["StartTime"]);
                var endTime = Convert.ToDateTime(requestData["EndTime"]);
                var excludeScheduleId = requestData.ContainsKey("ExcludeScheduleId")
                    ? (int?)Convert.ToInt32(requestData["ExcludeScheduleId"])
                    : null;

                var hasOverlap = await RosterServices.CheckOverlappingShifts(staffId, startTime, endTime, excludeScheduleId);
                var calculatedHours = RosterServices.CalculateHours(startTime, endTime);

                return Ok(new {
                    hasOverlap = hasOverlap,
                    calculatedHours = calculatedHours,
                    message = hasOverlap ? "Shift overlaps with existing schedule" : "No overlap detected"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/roster/calculate-hours - Calculates hours between times
        /// </summary>
        [HttpPost("calculate-hours")]
        public IActionResult CalculateHours([FromBody] object body)
        {
            try
            {
                var requestData = JsonConvert.DeserializeObject<Dictionary<string, object>>(body.ToString());

                if (!requestData.ContainsKey("StartTime") || !requestData.ContainsKey("EndTime"))
                {
                    return BadRequest(new { message = "StartTime and EndTime are required" });
                }

                var startTime = Convert.ToDateTime(requestData["StartTime"]);
                var endTime = Convert.ToDateTime(requestData["EndTime"]);

                var calculatedHours = RosterServices.CalculateHours(startTime, endTime);

                return Ok(new {
                    calculatedHours = calculatedHours,
                    startTime = startTime.ToString("yyyy-MM-dd HH:mm"),
                    endTime = endTime.ToString("yyyy-MM-dd HH:mm")
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        #endregion
    }
}