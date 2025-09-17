using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestfulAPI_FarmTimeManagement.Models;
using RestfulAPI_FarmTimeManagement.Services.Sprint_2.Tom; // EventServices
using RestfulAPI_FarmTimeManagement.Services.Sprint1.Tom;  // HistoryServices.GetClientIp
using RestfulAPI_FarmTimeManagement.Services.Sprint_2.Tim; // Tim's enhanced services
using RestfulAPI_FarmTimeManagement.DataConnects;          // Config
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace RestfulAPI_FarmTimeManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        // GET: api/events
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var rows = await EventServices.GetAllEvents();
            return new OkObjectResult(JsonConvert.SerializeObject(rows));
        }

        // POST: api/events/query
        // Body: chuỗi SQL SELECT tuỳ ý
        [HttpPost("query")]
        public async Task<IActionResult> QueryWithBody([FromBody] string query)
        {
            var rows = await EventServices.QueryEvents(query);
            return new OkObjectResult(JsonConvert.SerializeObject(rows));
        }

        // GET: api/events/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var rows = await EventServices.QueryEvents($@"SELECT * FROM [Event] WHERE EventId = {id}");
            var evt = rows.FirstOrDefault();
            return new OkObjectResult(JsonConvert.SerializeObject(evt));
        }



        // POST: api/events
        // Body: JSON của Event
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] object body)
        {
            var item = JsonConvert.DeserializeObject<Event>(body.ToString());
            var created = await EventServices.CreateEvent(item);
            return new OkObjectResult(JsonConvert.SerializeObject(created));
        }




        // POST: api/events
        // Body: JSON của Event
        [HttpPost("lockin")]
        public async Task<IActionResult> Create_lock_in([FromBody] object body)
        {
             
           Dictionary<string, int> dic = JsonConvert.DeserializeObject<Dictionary<string, int>>(body.ToString());

            int staffid = dic["staffid"];
            int deviceid = dic["deviceid"]; 

            var created = await EventServices.Create_Clock_in_Event(staffid,deviceid);


            return new OkObjectResult(JsonConvert.SerializeObject(created));
        }

        // POST: api/events
        // Body: JSON của Event
        [HttpPost("lockout")]
        public async Task<IActionResult> Create_lock_out([FromBody] object body)
        {

            Dictionary<string, int> dic = JsonConvert.DeserializeObject<Dictionary<string, int>>(body.ToString());

            int staffid = dic["staffid"];
            int deviceid = dic["deviceid"];

            var created = await EventServices.Create_Clock_out_Event(staffid, deviceid);


            return new OkObjectResult(JsonConvert.SerializeObject(created));
        }

        // Tim's Enhanced Endpoints - PBI 8.3.3 & 8.4.3 Implementation

        /// <summary>
        /// Tim's enhanced clock-in with roster validation
        /// POST: api/events/tim-lockin
        /// </summary>
        [HttpPost("tim-lockin")]
        public async Task<IActionResult> TimEnhancedClockIn([FromBody] object body)
        {
            try
            {
                Dictionary<string, object> dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(body.ToString());

                int staffid = Convert.ToInt32(dic["staffid"]);
                int deviceid = Convert.ToInt32(dic["deviceid"]);
                bool bypassValidation = dic.ContainsKey("bypassValidation") ? Convert.ToBoolean(dic["bypassValidation"]) : false;
                string overrideReason = dic.ContainsKey("overrideReason") ? dic["overrideReason"]?.ToString() : null;

                // Use Tim's roster validation service
                if (!bypassValidation)
                {
                    var validationResult = await RosterValidationService.ValidateClockIn(staffid);
                    if (!validationResult.IsValid)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = validationResult.Message,
                            validationCode = validationResult.ValidationCode,
                            rosterInfo = validationResult.RosterInfo,
                            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        });
                    }
                }
                else
                {
                    // Check admin privileges for override
                    var currentStaff = HttpContext.Items["Staff"] as Staff;
                    if (currentStaff?.Role != "Admin")
                    {
                        return Unauthorized(new
                        {
                            success = false,
                            message = "Only administrators can bypass roster validation",
                            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        });
                    }
                }

                // Proceed with Tom's existing clock-in logic
                var created = await EventServices.Create_Clock_in_Event(staffid, deviceid);

                if (created != null)
                {
                    // Tim's enhanced logging
                    var logMessage = bypassValidation
                        ? $"Enhanced clock-in with admin override. Reason: {overrideReason}"
                        : "Enhanced clock-in with roster validation";

                    await HistoryServices.CreateHistory(
                        "Tim Enhanced Clock-in",
                        "Success",
                        logMessage,
                        HttpContext
                    );

                    return Ok(new
                    {
                        success = true,
                        message = $"Clock-in recorded successfully at {created.Timestamp:HH:mm}",
                        eventId = created.EventId,
                        timestamp = created.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                        enhanced = true,
                        validationType = bypassValidation ? "admin_override" : "roster_validated"
                    });
                }

                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to create clock-in event",
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Enhanced clock-in failed: {ex.Message}",
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }

        /// <summary>
        /// Tim's enhanced clock-out with roster validation
        /// POST: api/events/tim-lockout
        /// </summary>
        [HttpPost("tim-lockout")]
        public async Task<IActionResult> TimEnhancedClockOut([FromBody] object body)
        {
            try
            {
                Dictionary<string, object> dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(body.ToString());

                int staffid = Convert.ToInt32(dic["staffid"]);
                int deviceid = Convert.ToInt32(dic["deviceid"]);
                bool bypassValidation = dic.ContainsKey("bypassValidation") ? Convert.ToBoolean(dic["bypassValidation"]) : false;
                string overrideReason = dic.ContainsKey("overrideReason") ? dic["overrideReason"]?.ToString() : null;

                // Use Tim's roster validation service
                if (!bypassValidation)
                {
                    var validationResult = await RosterValidationService.ValidateClockOut(staffid);
                    if (!validationResult.IsValid)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = validationResult.Message,
                            validationCode = validationResult.ValidationCode,
                            rosterInfo = validationResult.RosterInfo,
                            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        });
                    }
                }
                else
                {
                    // Check admin privileges for override
                    var currentStaff = HttpContext.Items["Staff"] as Staff;
                    if (currentStaff?.Role != "Admin")
                    {
                        return Unauthorized(new
                        {
                            success = false,
                            message = "Only administrators can bypass roster validation",
                            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        });
                    }
                }

                // Proceed with Tom's existing clock-out logic
                var created = await EventServices.Create_Clock_out_Event(staffid, deviceid);

                if (created != null)
                {
                    // Tim's enhanced logging
                    var logMessage = bypassValidation
                        ? $"Enhanced clock-out with admin override. Reason: {overrideReason}"
                        : "Enhanced clock-out with roster validation";

                    await HistoryServices.CreateHistory(
                        "Tim Enhanced Clock-out",
                        "Success",
                        logMessage,
                        HttpContext
                    );

                    return Ok(new
                    {
                        success = true,
                        message = $"Clock-out recorded successfully at {created.Timestamp:HH:mm}",
                        eventId = created.EventId,
                        timestamp = created.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                        enhanced = true,
                        validationType = bypassValidation ? "admin_override" : "roster_validated"
                    });
                }

                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to create clock-out event",
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Enhanced clock-out failed: {ex.Message}",
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }

        /// <summary>
        /// Tim's roster status check utility
        /// GET: api/events/roster-status/{staffId}
        /// </summary>
        [HttpGet("roster-status/{staffId}")]
        public async Task<IActionResult> GetRosterStatus(int staffId)
        {
            try
            {
                var hasRosterToday = await RosterValidationService.HasRosterToday(staffId);
                var nextShift = await RosterValidationService.GetNextScheduledShift(staffId);
                var staff = await StaffsServices.GetStaffById(staffId);

                return Ok(new
                {
                    staffId = staffId,
                    staffName = $"{staff.FirstName} {staff.LastName}",
                    hasRosterToday = hasRosterToday,
                    nextShift = nextShift != null ? new
                    {
                        date = nextShift.StartTime.ToString("yyyy-MM-dd"),
                        startTime = nextShift.StartTime.ToString("HH:mm"),
                        endTime = nextShift.EndTime.ToString("HH:mm"),
                        totalHours = nextShift.ScheduleHours
                    } : null,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error retrieving roster status: {ex.Message}",
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }

        /// <summary>
        /// Tim's validation preview (dry-run)
        /// POST: api/events/validate-timing
        /// </summary>
        [HttpPost("validate-timing")]
        public async Task<IActionResult> ValidateTiming([FromBody] object body)
        {
            try
            {
                Dictionary<string, object> dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(body.ToString());

                int staffid = Convert.ToInt32(dic["staffid"]);
                string action = dic["action"]?.ToString()?.ToLower(); // "clock-in" or "clock-out"
                DateTime? proposedTime = dic.ContainsKey("proposedTime")
                    ? DateTime.Parse(dic["proposedTime"].ToString())
                    : null;

                RosterValidationResult validationResult;

                if (action == "clock-in")
                {
                    validationResult = await RosterValidationService.ValidateClockIn(staffid, proposedTime);
                }
                else if (action == "clock-out")
                {
                    validationResult = await RosterValidationService.ValidateClockOut(staffid, proposedTime);
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid action. Must be 'clock-in' or 'clock-out'",
                        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }

                return Ok(new
                {
                    isValid = validationResult.IsValid,
                    message = validationResult.Message,
                    validationCode = validationResult.ValidationCode,
                    rosterInfo = validationResult.RosterInfo,
                    action = action,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Validation error: {ex.Message}",
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }










        [Authorize]
        // PUT: api/events/5
        // Body: JSON của Event
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] object body)
        {
             var item = JsonConvert.DeserializeObject<Event>(body.ToString());
            var updated = await EventServices.UpdateEvent(id, item,HttpContext);
            return new OkObjectResult(JsonConvert.SerializeObject(updated));
        }

        [Authorize]
        // DELETE: api/events/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
             var deleted = await EventServices.DeleteEvent(id,HttpContext);
            return new OkObjectResult(JsonConvert.SerializeObject(deleted));
        }
    }
}
