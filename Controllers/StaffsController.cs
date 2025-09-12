using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using RestfulAPI_FarmTimeManagement.DataConnects;
using RestfulAPI_FarmTimeManagement.Models; // Đổi "MyApi" thành namespace thực tế của bạn
using RestfulAPI_FarmTimeManagement.Services;
using RestfulAPI_FarmTimeManagement.Services.Sprint1.Tom;
using System.Data;

namespace RestfulAPI_FarmTimeManagement.Controllers // Đổi "MyApi" thành namespace thực tế của bạn
{
    [ApiController]
    [Route("api/[controller]")] 
    
    public class StaffsController : ControllerBase
    {



       
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] object body)
        {
             

            Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(body.ToString());

            string username = dic.ContainsKey("Email") ? dic["Email"] : "";
            string password = dic.ContainsKey("Password") ? dic["Password"] : "";

            Staff staff_login = await StaffsServices.Login(username, password,HttpContext);

            if (staff_login.StaffId == -1)
            {
                 return Unauthorized(new { message = staff_login.Email });
            }


            string token = TokenService.Generate(staff_login);

            return Ok(new
            {
                token,
                staff = staff_login
            });


            // return new OkObjectResult(JsonConvert.SerializeObject(staff_login)); 




        }








        // GET: api/staffs?query=SELECT * FROM Staff WHERE role='Admin'
        // hoặc để trống -> trả toàn bộ staff
       
        [HttpGet]
        public async Task<IActionResult> Query([FromQuery] string? query)
        { 
           List<Staff> staffs =  await StaffsServices.GetAllStaffs();  
           return new OkObjectResult(JsonConvert.SerializeObject(staffs)); 
         }

        
        [HttpPost("query")]
        public async Task<IActionResult> QueryWithBody([FromBody] string query)
        {

            List<Staff> staffs = await StaffsServices.QuerryStaffs(query);
            return new OkObjectResult(JsonConvert.SerializeObject(staffs));

        }




        // GET: api/staffs/5 
     
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            Staff staff = await StaffsServices.GetStaffById(id);
            return new OkObjectResult(JsonConvert.SerializeObject(staff));
        }


        [Authorize]
        // POST: api/staffs
        // Body: JSON object của Staff (password có thể null; service sẽ INSERT và trả JSON bản ghi mới)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] object body)
        {
 
            Staff staff = JsonConvert.DeserializeObject<Staff>(body.ToString()); 

            Staff staff_created = await StaffsServices.CreateStaff(staff,HttpContext);



            if (staff_created.StaffId == -1)
            {
                return Unauthorized(new { message = staff_created.Email });
            }



            return new OkObjectResult(JsonConvert.SerializeObject(staff_created));

        }

        [Authorize]
        // PUT: api/staffs/5
        // Body: JSON object của Staff (các trường sẽ được cập nhật đúng theo service)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] object body)
        {

 
            Staff staff = JsonConvert.DeserializeObject<Staff>(body.ToString());

            Staff staff_updated = await StaffsServices.UpdateStaff(id, staff,HttpContext);



            if (staff_updated.StaffId == -1)
            {
                return Unauthorized(new { message = staff_updated.Email });
            }




            return new OkObjectResult(JsonConvert.SerializeObject(staff_updated));
        }

        [Authorize]
        // DELETE: api/staffs/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
 

            Staff staff_deleted = await StaffsServices.DeleteStaff(id, HttpContext);


            if (staff_deleted.StaffId == -1)
            {
                return Unauthorized(new { message = staff_deleted.Email });
            }



            return new OkObjectResult(JsonConvert.SerializeObject(staff_deleted));
        }


      





    }
}
