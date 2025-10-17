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
        /// </summary>
        [HttpGet("staff/{staffId:int}")]
        public async Task<IActionResult> GetByStaffId(int staffId, [FromQuery] string? weekStartDate)
        {
            try
            {
                DateTime? parsedWeekStartDate = null;
                
                if (!string.IsNullOrEmpty(weekStartDate))
                {
                    if (!DateTime.TryParseExact(weekStartDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
                    {
                        return BadRequest(new { message = "weekStartDate must be in yyyy-MM-dd format" });
                    }
                    parsedWeekStartDate = parsedDate;
                }

                var schedules = await RosterServices.GetSchedulesByStaffId(staffId, parsedWeekStartDate);
                return new OkObjectResult(JsonConvert.SerializeObject(schedules));
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