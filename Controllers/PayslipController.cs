using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestfulAPI_FarmTimeManagement.Services.Sprint_4.Tom;

namespace RestfulAPI_FarmTimeManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PayslipController : ControllerBase
    {
        

        /// <summary>
        /// POST: api/payslip/create - Creates a new payslip for a staff member for a specific week or returns existing payslip
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreatePayslip([FromBody] CreatePayslipRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Request body is required");
                }

                if (request.StaffId <= 0)
                {
                    return BadRequest("StaffId must be greater than 0");
                }

                if (request.WeekStartDate == default(DateTime))
                {
                    return BadRequest("WeekStartDate is required");
                }

                var payslip = await PayslipServices.CreatePayslip(request.StaffId, request.WeekStartDate);

                if (payslip == null)
                {
                    return BadRequest("Failed to create payslip. Staff may not exist.");
                }

                return Ok(payslip);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// DELETE: api/payslip/{payslipId} - Deletes a payslip by ID
        /// </summary>
        [HttpDelete("{payslipId:int}")]
        public async Task<IActionResult> DeletePayslip(int payslipId)
        {
            try
            {
                if (payslipId <= 0)
                {
                    return BadRequest("PayslipId must be greater than 0");
                }

                var deletedPayslip = await PayslipServices.DeletePayslip(payslipId);

                if (deletedPayslip == null)
                {
                    return NotFound("Payslip not found");
                }

                return Ok(deletedPayslip);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// GET: api/payslip - Gets all payslips
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> QueryPayslips()
        {
            try
            {
                var payslips = await PayslipServices.QueryPayslips();
                return Ok(payslips);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// GET: api/payslip/staff/{staffId} - Gets all payslips for a specific staff member
        /// </summary>
        [HttpGet("staff/{staffId:int}")]
        public async Task<IActionResult> GetPayslipsByStaffId(int staffId)
        {
            try
            {
                if (staffId <= 0)
                {
                    return BadRequest("StaffId must be greater than 0");
                }

                var payslips = await PayslipServices.GetPayslipsByStaffId(staffId);
                return Ok(payslips);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }

    public class CreatePayslipRequest
    {
        public int StaffId { get; set; }
        public DateTime WeekStartDate { get; set; }
    }
}
