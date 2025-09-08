using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using RestfulAPI_FarmTimeManagement.DataConnects;
using RestfulAPI_FarmTimeManagement.Models;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace RestfulAPI_FarmTimeManagement.Services.Sprint1.Tom
{
    public static class StaffsServices
    {



        public static async Task<Staff> Login(string username, string password)
        {

            StaffConnects staffsService = new StaffConnects();

            password = hashPassword(password);

            var querystring = $@"SELECT * FROM Staff WHERE Email = '{username}' AND Password = '{password}'";

            List<Staff> result = await staffsService.QueryStaff(querystring);
            if (result.Count == 1)
            {

                Staff staff = result[0];

                if (staff.Role !="Admin")
                {
                    History history_notadmin = new History
                    {
                        Action = "Login",
                        Details = "User was not admin",
                        Actor = username,
                        Result = "Failed",
                    };
                    History result_notadmin = await HistoryServices.CreateHistory(history_notadmin);
                    throw new Exception($"User {staff.FirstName} {staff.LastName} was not admin");

                }

                History history = new History
                {
                    Action = "Login",
                    Details = $"user {staff.FirstName} {staff.LastName} logged in",
                    Actor = staff.Email,
                    Result = "Succeed",
                };
                History result_hiscreated = await HistoryServices.CreateHistory(history);
                if (result_hiscreated != null)
                {
                    return staff;

                }

            }
            else
            {
                History history = new History
                {
                    Action = "Login",
                    Details = "Wrong username or password",
                    Actor = username,
                    Result = "Failed",
                };
                History result_hiscreated = await HistoryServices.CreateHistory(history);
            }

            throw new Exception("Wrong username or password");


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



        public static async Task<Staff> CreateStaff(Staff staff)
        {
            StaffConnects staffsService = new StaffConnects();
 
            bool isStaffexsit = await is_Staff_exist(staff.Email);
            if (isStaffexsit)
            {
                throw new Exception("This email was registered before");
            }
            else
            {

                staff.Password = hashPassword(staff.Password);
                var result = await staffsService.CreateStaff(staff);


                if (result != null)
                {
                    History history = new History
                    {
                        Action = "Create user",
                        Details = $"user {staff.FirstName} {staff.LastName} was created.",
                        Actor = staff.Email,
                        Result = "Succeed",
                    };
                    History result_hiscreated = await HistoryServices.CreateHistory(history);

                    if (result_hiscreated != null)
                    {
                        return result;

                    }

                }
            }
            throw new Exception("Create staff failed");
        }




        public static async Task<Staff> UpdateStaff(int id, Staff staff)
        {
            StaffConnects staffConnects = new StaffConnects();
 
            var existingStaff = await GetStaffById(id);
            if (existingStaff == null)
            {
                throw new Exception("Staff not found");
            }

            staff.Password = hashPassword(staff.Password);

            var result = await staffConnects.UpdateStaff(id, staff);
            if (result != null)
            {
                History history = new History
                {
                    Action = "Update user",
                    Details = $"user {staff.FirstName} {staff.LastName} was updated.",
                    Actor = staff.Email,
                    Result = "Succeed",
                };
                History result_hiscreated = await HistoryServices.CreateHistory(history);

                if (result_hiscreated != null)
                {
                    return result; 
                } 
            }

            throw new Exception("Update staff failed");
        } 


        public static async Task<Staff> DeleteStaff(int id)
        {
            StaffConnects staffConnects = new StaffConnects();
 


            var existingStaff = await GetStaffById(id);
            if (existingStaff == null)
            {
              

                throw new Exception("Staff not found");
            }
            var result = await staffConnects.DeleteStaff(id);

            if (result != null)
            {
                History history = new History
                {
                    Action = "Delete user",
                    Details = $"user {result.FirstName} {result.LastName} was deleted.",
                    Actor = result.Email,
                    Result = "Succeed",
                };
                History result_hiscreated = await HistoryServices.CreateHistory(history);

                if (result_hiscreated != null)
                {
                    return result;

                } 
            }

            throw new Exception("Delete staff failed");
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

