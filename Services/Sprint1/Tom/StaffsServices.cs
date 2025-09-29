using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using RestfulAPI_FarmTimeManagement.DataConnects;
using RestfulAPI_FarmTimeManagement.Models;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RestfulAPI_FarmTimeManagement.Services.Sprint1.Tom
{
    public static class StaffsServices
    {



        public static async Task<Staff> Login(string username, string password, HttpContext httpContext)
        {

            StaffConnects staffsService = new StaffConnects();

            password = hashPassword(password);


            Staff staff = new Staff
            {
                Email = username,
                Password = password
            };

            httpContext.Items["Staff"] = staff;







            var querystring = $@"SELECT * FROM Staff WHERE Email = '{username}' AND Password = '{password}'";

            List<Staff> result = await staffsService.QueryStaff(querystring);
            if (result.Count == 1)
            {

                 staff = result[0];

                if (staff.Role !="Admin")
                {
                    History history_notadmin = new History
                    {
                        Action = "Login",
                        Details = "User was not admin",
                        Actor = username,
                        Result = "Failed",
                    }; 

                    History result_notadmin = await HistoryServices.
                        CreateHistory("Login", "Failed", "User was not admin", httpContext); 

                    return new Staff { StaffId = -1, Email = "User was not admin"};
                   
                }
                History result_hiscreated = await HistoryServices.
                     CreateHistory("Login", "Succeed", $"user {staff.FirstName} {staff.LastName} logged in", httpContext);
                if (result_hiscreated != null)
                {
                    return staff;
                }

            }
            else
            {
               
                History result_hiscreated = await HistoryServices.
                        CreateHistory("Login", "Failed", $"Wrong username or password", httpContext); 
                return new Staff { StaffId = -1, Email = "Wrong username or password" };


            }
            return new Staff { StaffId = -1, Email = "Failed due to system issue." };

        }





        public static async Task<List<Staff>> GetAllStaffs()
        {
            StaffConnects staffsService = new StaffConnects();
            var querystring = $@"SELECT * FROM Staff";
            List<Staff> result = await staffsService.QueryStaff(querystring);
            return result;
        }




        public static async Task<List<Staff>> QuerryStaffs(string querystring)
        {
            StaffConnects staffsService = new StaffConnects();
            List<Staff> result = await staffsService.QueryStaff(querystring);
            return result;
        }
         
        public static async Task<Staff> GetStaffById(int id)
        {
            StaffConnects staffsService = new StaffConnects();

            var querystring = $@"SELECT * FROM Staff WHERE StaffId = {id}";
            List<Staff> result = await staffsService.QueryStaff(querystring);
            if (result.Count == 1)
            {
                Staff staff = result[0];
                staff.Password = null; // do not return password
                return staff;
            }
            throw new Exception("Staff not found");


        }



        public static async Task<Staff> CreateStaff(Staff staff, HttpContext httpContext)
        {
            StaffConnects staffsService = new StaffConnects(); 

            bool isStaffexsit = await is_Staff_exist(staff.Email);
            if (isStaffexsit)
            { 
                return new Staff { StaffId = -1, Email = "This email was registered before" }; 
            }
            else
            { 
 
 
                staff.Password = hashPassword(staff.Password);
                var result = await staffsService.CreateStaff(staff);
                 
                if (result != null)
                {
              

                    History result_hiscreated = await HistoryServices.
                        CreateHistory("Create user", "Succeed", $"user {staff.FirstName} {staff.LastName} was created.", httpContext);

                     
                    if (result_hiscreated != null)
                    {
                        return result;

                    }

                }
            }
            return new Staff { StaffId = -1, Email = "Failed due to system issue." };
        }




        public static async Task<Staff> UpdateStaff(int id, Staff staff,HttpContext httpContext)
        {


            bool isStaffexsit = await is_Staff_exist(staff.Email);
            if (isStaffexsit)
            {
                return new Staff { StaffId = -1, Email = "This email was registered before" };
            }




            StaffConnects staffConnects = new StaffConnects();
 
            var existingStaff = await GetStaffById(id);
            if (existingStaff == null)
            {
                return new Staff { StaffId = -1, Email = "Staff was not found" };
            }

            // Get current password using QuerryStaffs function 
            staff.Password = existingStaff.Password;
 
         

            var result = await staffConnects.UpdateStaff(id, staff);
            if (result != null)
            {
                 

                History result_hiscreated = await HistoryServices.
                CreateHistory("Update user", "Succeed", $"user {staff.FirstName} {staff.LastName} was updated.", httpContext);
                 

                if (result_hiscreated != null)
                {
                    return result; 
                } 
            }

            return new Staff { StaffId = -1, Email = "Failed due to system issue." };
        }


        public static async Task<Staff> DeleteStaff(int id, HttpContext httpContext)
        {
            StaffConnects staffConnects = new StaffConnects(); 

            var existingStaff = await GetStaffById(id);
            if (existingStaff == null)
            { 
                return new Staff { StaffId = -1, Email = "Staff was not found" };
            }
            var result = await staffConnects.DeleteStaff(id);

            if (result != null)
            { 
 
                History result_hiscreated = await HistoryServices.
                 CreateHistory("Delete user", "Succeed", $"user {result.FirstName} {result.LastName} was deleted.", httpContext);
                 

                if (result_hiscreated != null)
                {
                    return result;

                } 
            }

            return new Staff { StaffId = -1, Email = "Failed due to system issue." };
        }

        public static async Task<Staff> ChangePasswordStaff(int id, string newpassword, HttpContext httpContext)
        {
            StaffConnects staffConnects = new StaffConnects();

            var existingStaff = await GetStaffById(id);
            if (existingStaff == null)
            {
                return new Staff { StaffId = -1, Email = "Staff was not found" };
            }

            // Hash the new password
            string hashedNewPassword = hashPassword(newpassword);


            existingStaff.Password = hashedNewPassword;

 

            var result = await staffConnects.UpdateStaff(id, existingStaff);
            if (result != null)
            {
                History result_hiscreated = await HistoryServices.
                    CreateHistory("Change password", "Succeed", $"Password for user {existingStaff.FirstName} {existingStaff.LastName} was changed.", httpContext);

                if (result_hiscreated != null)
                {
                    return result;
                }
            }

            return new Staff { StaffId = -1, Email = "Failed due to system issue." };
        }


        // CHECK — check whether this email was resstered before
        private static async Task<bool> is_Staff_exist(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new Exception("wrong email");

            string sql = $@"SELECT * FROM Staff WHERE Email = '{email}'";

            StaffConnects staffConnects = new StaffConnects();

            List<Staff> result = await staffConnects.QueryStaff(sql);
            if (result.Count == 0)
            {
                return false;
            }

            return true;
        }


        // Hash password before storing to database
        private static string hashPassword(string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2")); // format hex
                }

                return sb.ToString();
            }


        }
    }
}

