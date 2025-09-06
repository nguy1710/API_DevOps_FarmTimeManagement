using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RestfulAPI_FarmTimeManagement.Models; // Đổi "MyApi" thành namespace thực tế của bạn
using RestfulAPI_FarmTimeManagement.Services;
using System.Data;

namespace RestfulAPI_FarmTimeManagement.Controllers // Đổi "MyApi" thành namespace thực tế của bạn
{
    [ApiController]
    [Route("api/[controller]")]
    public class StaffsController : ControllerBase
    {


        private readonly StaffsService _svc;
        public StaffsController(StaffsService svc)
        {
            _svc = svc;
        }

        // GET: api/staffs?query=SELECT * FROM Staff WHERE role='Admin'
        // hoặc để trống -> trả toàn bộ staff
        [HttpGet]
        public async Task<IActionResult> Query([FromQuery] string? query)
        {
            var json = await _svc.QueryStaffAsync(query, HttpContext.RequestAborted);
            return Content(json, "application/json");
        }


        [HttpPost("query")]
        public async Task<IActionResult> QueryWithBody([FromBody] string query)
        { 

            var json = await _svc.QueryStaffAsync(query, HttpContext.RequestAborted);
            return Content(json, "application/json");
        }
         



        // GET: api/staffs/5  (dùng QueryStaffAsync phía service)
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var sql = $"SELECT * FROM Staff WHERE staffId = {id}";
            var json = await _svc.QueryStaffAsync(sql, HttpContext.RequestAborted);
            return Content(json, "application/json");
        }

        // POST: api/staffs
        // Body: JSON object của Staff (password có thể null; service sẽ INSERT và trả JSON bản ghi mới)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] object body)
        {
            var jsonBody = body?.ToString() ?? "{}";
            var json = await _svc.CreateStaffAsync(jsonBody, HttpContext.RequestAborted);
            return Content(json, "application/json");
        }

        // PUT: api/staffs/5
        // Body: JSON object của Staff (các trường sẽ được cập nhật đúng theo service)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] object body)
        {
            var jsonBody = body?.ToString() ?? "{}";
            var json = await _svc.UpdateStaffAsync(id, jsonBody, HttpContext.RequestAborted);
            return Content(json, "application/json");
        }

        // DELETE: api/staffs/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var json = await _svc.DeleteStaffAsync(id, HttpContext.RequestAborted);
            return Content(json, "application/json");
        }

         


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] object body)
        {
            var jsonBody = body?.ToString() ?? "{}";

            var json = await _svc.LoginAsync(jsonBody, HttpContext, HttpContext.RequestAborted);


            if(!string.IsNullOrEmpty(json))
            {
              //  return Content(json, "application/json");
                return new OkObjectResult(json);
            }


            return new BadRequestObjectResult("Wrong username or password!");




           
        }









    }
}
