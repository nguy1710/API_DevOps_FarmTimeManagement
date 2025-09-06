using Microsoft.AspNetCore.Mvc;
using RestfulAPI_FarmTimeManagement.Models;
using RestfulAPI_FarmTimeManagement.Services;
using System.Net;
using System.Text.Json;

namespace RestfulAPI_FarmTimeManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HistoriesController : ControllerBase
    {
        private readonly HistoriesService _svc;
        public HistoriesController(HistoriesService svc)
        {
            _svc = svc;
        }

        [HttpGet]
        public async Task<IActionResult> Query([FromQuery] string? query)
        {
            var json = await _svc.QueryHistoriesAsync(query, HttpContext.RequestAborted);
            return Content(json, "application/json");
        }

        [HttpPost("query")]
        public async Task<IActionResult> QueryWithBody([FromBody] string query)
        {
            var json = await _svc.QueryHistoriesAsync(query, HttpContext.RequestAborted);
            return Content(json, "application/json");
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var sql = $"SELECT * FROM History WHERE HistoryId = {id}";
            var json = await _svc.QueryHistoriesAsync(sql, HttpContext.RequestAborted);
            return Content(json, "application/json");
        }

        // ====== Tự gắn IP & Timestamp khi tạo ======
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] object body)
        {
            // 1) Lấy raw JSON string
            var jsonBody = body?.ToString() ?? "{}";

          
            var resultJson = await _svc.CreateHistoryAsync(jsonBody, HttpContext , HttpContext.RequestAborted);
            return Content(resultJson, "application/json");
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var json = await _svc.DeleteHistoryAsync(id, HttpContext.RequestAborted);
            return Content(json, "application/json");
        }


    }
}
