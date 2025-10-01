using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestfulAPI_FarmTimeManagement.Models;
using RestfulAPI_FarmTimeManagement.Services.Sprint3.Tan;

namespace RestfulAPI_FarmTimeManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PayRatesController : ControllerBase
    {
        [HttpGet("defaults")]
        public async Task<IActionResult> GetAllDefaultPayRates()
        {
            var defaultRates = PayRateServices.GetDefaultPayRates();
            return new OkObjectResult(JsonConvert.SerializeObject(defaultRates));
        }

        [Authorize]
        [HttpPut("staff/{id:int}/payrates")]
        public async Task<IActionResult> UpdateStaffPayRates(int id, [FromBody] object body)
        {
            try
            {
                // Get current staff from middleware
                var currentStaff = HttpContext.Items["Staff"] as Staff;
                if (currentStaff == null)
                {
                    return Unauthorized(new { message = "Authentication required" });
                }

                // Check if user is admin
                if (currentStaff.Role != "Admin")
                {
                    return Forbid("Only admin users can update pay rates");
                }

                var payRateUpdate = JsonConvert.DeserializeObject<PayRateUpdateRequest>(body.ToString()!);

                Staff? updatedStaff = await PayRateServices.UpdateStaffPayRates(
                    id, payRateUpdate.StandardPayRate, payRateUpdate.OvertimePayRate);

                if (updatedStaff == null)
                {
                    return BadRequest(new { message = "Failed to update staff pay rates" });
                }

                return new OkObjectResult(new
                {
                    message = "Staff pay rates updated successfully",
                    staff = JsonConvert.SerializeObject(updatedStaff)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("bulk/{role}/{contractType}")]
        public async Task<IActionResult> BulkUpdatePayRates(string role, string contractType, [FromBody] object body)
        {
            try
            {
                // Get current staff from middleware
                var currentStaff = HttpContext.Items["Staff"] as Staff;
                if (currentStaff == null)
                {
                    return Unauthorized(new { message = "Authentication required" });
                }

                // Check if user is admin
                if (currentStaff.Role != "Admin")
                {
                    return Forbid("Only admin users can update pay rates");
                }

                var payRateUpdate = JsonConvert.DeserializeObject<PayRateUpdateRequest>(body.ToString()!);

                List<Staff> updatedStaff = await PayRateServices.BulkUpdatePayRatesByRoleContract(
                    role, contractType, payRateUpdate.StandardPayRate, payRateUpdate.OvertimePayRate);

                return new OkObjectResult(new
                {
                    message = $"Successfully updated {updatedStaff.Count} staff members with role '{role}' and contract type '{contractType}'",
                    staff = JsonConvert.SerializeObject(updatedStaff)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("initialize-defaults")]
        public async Task<IActionResult> InitializeDefaultPayRates()
        {
            try
            {
                // Get current staff from middleware
                var currentStaff = HttpContext.Items["Staff"] as Staff;
                if (currentStaff == null)
                {
                    return Unauthorized(new { message = "Authentication required" });
                }

                // Check if user is admin
                if (currentStaff.Role != "Admin")
                {
                    return Forbid("Only admin users can initialize default pay rates");
                }

                List<Staff> updatedStaff = await PayRateServices.InitializeDefaultPayRates();

                return new OkObjectResult(new
                {
                    message = $"Successfully updated {updatedStaff.Count} staff members with default Horticulture Award 2025 pay rates",
                    staff = JsonConvert.SerializeObject(updatedStaff)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class PayRateUpdateRequest
    {
        public decimal StandardPayRate { get; set; }
        public decimal OvertimePayRate { get; set; }
    }
}