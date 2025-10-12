using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using RestfulAPI_FarmTimeManagement.Models;
using System.Data;

 


namespace RestfulAPI_FarmTimeManagement.DataConnects
{
    public class StaffConnects
    {
       
    
        private SqlConnection Conn() => new SqlConnection(Config._connString);

        
         private static T? GetNullable<T>(SqlDataReader r, int i) => r.IsDBNull(i) ? default : r.GetFieldValue<T>(i);

         private static Staff MapStaff(SqlDataReader rd)
        {
            int iId = rd.GetOrdinal("StaffId");
            int iFn = rd.GetOrdinal("FirstName");
            int iLn = rd.GetOrdinal("LastName");
            int iEmail = rd.GetOrdinal("Email");
            int iPhone = rd.GetOrdinal("Phone");
            int iAddr = rd.GetOrdinal("Address");
            int iCt = rd.GetOrdinal("ContractType");
            int iRole = rd.GetOrdinal("Role");
            int iStdP = rd.GetOrdinal("StandardPayRate");
            int iPwd = rd.GetOrdinal("Password");

            return new Staff
            {
                StaffId = rd.GetInt32(iId),
                FirstName = rd.GetString(iFn),
                LastName = rd.GetString(iLn),
                Email = GetNullable<string>(rd, iEmail),
                Phone = GetNullable<string>(rd, iPhone),
                Password = GetNullable<string>(rd, iPwd),
                Address = GetNullable<string>(rd, iAddr),
                ContractType = GetNullable<string>(rd, iCt),
                Role = GetNullable<string>(rd, iRole),
                StandardPayRate = GetNullable<decimal>(rd, iStdP)
            };
        }




        #region CRUD STAFF

        public async Task<List<Staff>> QueryStaff(string query_stringSQL)
        {
            if (string.IsNullOrWhiteSpace(query_stringSQL))
                query_stringSQL = "SELECT * FROM Staff";

            var list = new List<Staff>();

            await using var conn = Conn();
            await conn.OpenAsync();   
            await using var cmd = new SqlCommand(query_stringSQL, conn);
            await using var rd = await cmd.ExecuteReaderAsync();  

            while (await rd.ReadAsync()) 
                list.Add(MapStaff(rd)); 

            return list;
        }


         public async Task<Staff> CreateStaff(Staff staff)
        {
            const string sql = @"
INSERT INTO Staff
(FirstName, LastName, Email, Phone, Address, ContractType, Role, StandardPayRate, Password)
OUTPUT INSERTED.*
VALUES
(@FirstName, @LastName, @Email, @Phone, @Address, @ContractType, @Role, @StandardPayRate, @Password);";

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);

            // Tên tham số khớp y hệt tên cột
            cmd.Parameters.AddWithValue("@FirstName", staff.FirstName);
            cmd.Parameters.AddWithValue("@LastName", staff.LastName);
            cmd.Parameters.AddWithValue("@Email", (object?)staff.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone", (object?)staff.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object?)staff.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ContractType", (object?)staff.ContractType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Role", (object?)staff.Role ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StandardPayRate", (object?)staff.StandardPayRate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Password", (object?)staff.Password ?? DBNull.Value);

            await using var rd = await cmd.ExecuteReaderAsync();
            Staff created = await rd.ReadAsync() ? MapStaff(rd) : null;

            return created;
        }


         public async Task<Staff> UpdateStaff(int id, Staff staff)
        {
            const string sql = @"
UPDATE Staff
SET FirstName       = @FirstName,
    LastName        = @LastName,
    Email           = @Email,
    Phone           = @Phone,
    Address         = @Address,
    ContractType    = @ContractType,
    Role            = @Role,
    StandardPayRate = @StandardPayRate,
    Password        = @Password
OUTPUT INSERTED.*
WHERE StaffId = @StaffId;";

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@StaffId", id);
            cmd.Parameters.AddWithValue("@FirstName", staff.FirstName);
            cmd.Parameters.AddWithValue("@LastName", staff.LastName);
            cmd.Parameters.AddWithValue("@Email", (object?)staff.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone", (object?)staff.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object?)staff.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ContractType", (object?)staff.ContractType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Role", (object?)staff.Role ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StandardPayRate", (object?)staff.StandardPayRate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Password", (object?)staff.Password ?? DBNull.Value);

            await using var rd = await cmd.ExecuteReaderAsync();
            var updated = await rd.ReadAsync() ? MapStaff(rd) : null;
            return updated!;
        }


         public async Task<Staff> DeleteStaff(int id)
        {
            const string sql = @"DELETE FROM Staff OUTPUT DELETED.* WHERE StaffId = @id;";

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await using var rd = await cmd.ExecuteReaderAsync();
            var deleted = await rd.ReadAsync() ? MapStaff(rd) : null;

            return deleted;
        }

        #endregion
         



    }







}


