using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestfulAPI_FarmTimeManagement.DataConnects;          // Config
using RestfulAPI_FarmTimeManagement.Models;
using RestfulAPI_FarmTimeManagement.Services.Sprint_2.Tom; // BiometricServices
using RestfulAPI_FarmTimeManagement.Services.Sprint1.Tom;  // HistoryServices
using System.Linq;

namespace RestfulAPI_FarmTimeManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
     

    public class BiometricsController : ControllerBase
    {
        [AllowAnonymous]
        // GET: api/biometrics
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var rows = await BiometricServices.GetAllBiometrics();
            return new OkObjectResult(JsonConvert.SerializeObject(rows));
        }

        [AllowAnonymous]
        // POST: api/biometrics/query
        // Body: chuỗi SQL SELECT tuỳ ý
        [HttpPost("query")]
        public async Task<IActionResult> QueryWithBody([FromBody] string query)
        {
            var rows = await BiometricServices.QueryBiometrics(query);
            return new OkObjectResult(JsonConvert.SerializeObject(rows));
        }

        [AllowAnonymous]
        // GET: api/biometrics/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var rows = await BiometricServices.QueryBiometrics($@"SELECT * FROM Biometric WHERE BiometricId = {id}");
            var bio = rows.FirstOrDefault();
            return new OkObjectResult(JsonConvert.SerializeObject(bio));
        }




        [AllowAnonymous]
        // GET: api/biometrics/5
        [HttpGet("scanfromcard/{result}")]
        public async Task<IActionResult> GetStaff_fromCard(string result)
        {
            Staff staff = await BiometricServices.GetStaff_From_Bio_Card(result); 

            return new OkObjectResult(JsonConvert.SerializeObject(staff));
        }




        // POST: api/biometrics
        // Body: JSON của Biometric
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] object body)
        {
             var item = JsonConvert.DeserializeObject<Biometric>(body.ToString());
            var created = await BiometricServices.CreateBiometric(item,HttpContext);

            if (created.BiometricId == -1)
            {
                return Unauthorized(new { message = created.Data });
            }

            return new OkObjectResult(JsonConvert.SerializeObject(created));
        }

        // PUT: api/biometrics/5
        // Body: JSON của Biometric
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] object body)
        {
             var item = JsonConvert.DeserializeObject<Biometric>(body.ToString());
            var updated = await BiometricServices.UpdateBiometric(id, item,HttpContext);


            if (updated.BiometricId == -1)
            {
                return Unauthorized(new { message = updated.Data });
            }

            return new OkObjectResult(JsonConvert.SerializeObject(updated));
        }

        // DELETE: api/biometrics/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
             var deleted = await BiometricServices.DeleteBiometric(id,HttpContext);


            if (deleted.BiometricId == -1)
            {
                return Unauthorized(new { message = deleted.Data });
            }

            return new OkObjectResult(JsonConvert.SerializeObject(deleted));
        }
    }
}
