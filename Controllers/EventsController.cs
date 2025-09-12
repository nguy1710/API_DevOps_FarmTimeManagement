using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestfulAPI_FarmTimeManagement.Models;
using RestfulAPI_FarmTimeManagement.Services.Sprint_2.Tom; // EventServices
using RestfulAPI_FarmTimeManagement.Services.Sprint1.Tom;  // HistoryServices.GetClientIp
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
