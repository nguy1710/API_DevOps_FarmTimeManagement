using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestfulAPI_FarmTimeManagement.Models;
using RestfulAPI_FarmTimeManagement.Services;
using RestfulAPI_FarmTimeManagement.Services.Sprint1.Tom;
using System.Net;
using System.Text.Json;

namespace RestfulAPI_FarmTimeManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HistoriesController : ControllerBase
    {
        

        [HttpGet]
        public async Task<IActionResult> Query([FromQuery] string? query)
        {
            List<History> histories = await HistoryServices.GetAllHistories();
            return new OkObjectResult(JsonConvert.SerializeObject(histories));


        }

        [HttpPost("query")]
        public async Task<IActionResult> QueryWithBody([FromBody] string query)
        {
            List<History> histories = await HistoryServices.QueryHistories(query);
            return new OkObjectResult(JsonConvert.SerializeObject(histories));

        }



        //[HttpGet("{id:int}")]
        //public async Task<IActionResult> GetById(int id)
        //{
          
        //}



        [HttpPost]
        public async Task<IActionResult> Create([FromBody] object body)
        {
            History history = JsonConvert.DeserializeObject<History>(body.ToString());


            History history_created = await HistoryServices.CreateHistory(history);

            return new OkObjectResult(JsonConvert.SerializeObject(history_created));
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
 
            History history_deleted = await HistoryServices.DeleteHistory(id);

            return new OkObjectResult(JsonConvert.SerializeObject(history_deleted));
        }


    }
}
