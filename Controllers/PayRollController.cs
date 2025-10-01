using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestfulAPI_FarmTimeManagement.Models;
using RestfulAPI_FarmTimeManagement.Services.Sprint_3.Tom;
using RestfulAPI_FarmTimeManagement.Services.Sprint3.Tan;

namespace RestfulAPI_FarmTimeManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PayRollController : ControllerBase
    {
        // GET: api/payroll/calculate?staffId=1&mondayDate=2024-12-30&isSpecialPayRate=true
        [HttpGet("calculate")]
        public async Task<IActionResult> CalculatePayroll(
            [FromQuery] int staffId, 
            [FromQuery] string mondayDate, 
            [FromQuery] bool? isSpecialPayRate = false)
        {
            try
            {
                // Validate staff ID
                if (staffId <= 0)
                {
                    return BadRequest(new { message = "Staff ID must be greater than 0" });
                }

                // Validate Monday date
                if (string.IsNullOrEmpty(mondayDate))
                {
                    return BadRequest(new { message = "Monday date is required" });
                }

                // Get staff by ID
                Staff? staff = await PayRateServices.GetStaffById(staffId);
                
                if (staff == null)
                {
                    return NotFound(new { message = $"Staff with ID {staffId} not found" });
                }

                // Parse Monday date
                if (!DateTime.TryParse(mondayDate, out DateTime parsedMondayDate))
                {
                    return BadRequest(new { message = "Invalid date format. Use format: yyyy-MM-dd" });
                }

                // Calculate payroll
                bool useSpecialPayRate = isSpecialPayRate ?? false;
                PayrollSummary payrollSummary = await PayRollServices.CalculateCompletePayroll(
                    parsedMondayDate, 
                    staff, 
                    useSpecialPayRate
                );

                // Check if there was an error (StaffId = -1)
                if (payrollSummary.StaffId == -1)
                {
                    return BadRequest(new 
                    { 
                        message = payrollSummary.StaffName,
                        weekStartDate = payrollSummary.WeekStartDate
                    });
                }

                // Return successful payroll calculation
                return Ok(payrollSummary);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

