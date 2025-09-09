using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestfulAPI_FarmTimeManagement.Models;
using RestfulAPI_FarmTimeManagement.Services.Sprint_2.Tom; // DeviceServices
using RestfulAPI_FarmTimeManagement.Services.Sprint1.Tom;  // HistoryServices để lấy client IP (nếu bạn đặt ở namespace này)
using RestfulAPI_FarmTimeManagement.DataConnects;          // Config nếu bạn đang lưu ở đây
using System.Linq;

namespace RestfulAPI_FarmTimeManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevicesController : ControllerBase
    {
        // GET: api/devices
        // (giống StaffsController: GET mặc định trả toàn bộ, không dùng querystring)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var devices = await DeviceServices.GetAllDevices();
            return new OkObjectResult(JsonConvert.SerializeObject(devices));
        }

        // POST: api/devices/query
        // Body: chuỗi SQL SELECT tuỳ ý
        [HttpPost("query")]
        public async Task<IActionResult> QueryWithBody([FromBody] string query)
        {
            var devices = await DeviceServices.QueryDevices(query);
            return new OkObjectResult(JsonConvert.SerializeObject(devices));
        }

        // GET: api/devices/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            // Hiện tại DeviceServices chưa có GetById; dùng QueryDevices để lấy theo id
            var rows = await DeviceServices.QueryDevices($@"SELECT * FROM Device WHERE DeviceId = {id}");
            var device = rows.FirstOrDefault();
            return new OkObjectResult(JsonConvert.SerializeObject(device));
        }

        // POST: api/devices
        // Body: JSON của Device
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] object body)
        {
            Config.client_ip = HistoryServices.GetClientIp(HttpContext);
            var device = JsonConvert.DeserializeObject<Device>(body.ToString());
            var created = await DeviceServices.CreateDevice(device);
            return new OkObjectResult(JsonConvert.SerializeObject(created));
        }

        // PUT: api/devices/5
        // Body: JSON của Device (các trường sẽ được cập nhật theo service)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] object body)
        {
            Config.client_ip = HistoryServices.GetClientIp(HttpContext);
            var device = JsonConvert.DeserializeObject<Device>(body.ToString());
            var updated = await DeviceServices.UpdateDevice(id, device);
            return new OkObjectResult(JsonConvert.SerializeObject(updated));
        }

        // DELETE: api/devices/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            Config.client_ip = HistoryServices.GetClientIp(HttpContext);
            var deleted = await DeviceServices.DeleteDevice(id);
            return new OkObjectResult(JsonConvert.SerializeObject(deleted));
        }
    }
}
