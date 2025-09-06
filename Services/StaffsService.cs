using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using RestfulAPI_FarmTimeManagement.Models;
using System.Data;

 


namespace RestfulAPI_FarmTimeManagement.Services
{
    public class StaffsService
    {
        private readonly string _connStr;
        private readonly HistoriesService _hist;

        public StaffsService(IConfiguration cfg, HistoriesService hist)
        {
            _connStr = cfg.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");
            _hist = hist;
        }

        private SqlConnection Conn() => new SqlConnection(_connStr);

        
        // ===== Mapper & helpers =====
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
            int iStdH = rd.GetOrdinal("StandardHoursPerWeek");
            int iStdP = rd.GetOrdinal("StandardPayRate");
            int iOvtP = rd.GetOrdinal("OvertimePayRate");
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
                StandardHoursPerWeek = GetNullable<decimal>(rd, iStdH),
                StandardPayRate = GetNullable<decimal>(rd, iStdP),
                OvertimePayRate = GetNullable<decimal>(rd, iOvtP)
            };
        }

        // ===================== ASYNC CRUD (JSON) =====================

        // READ (Query) — nếu query null/empty thì trả toàn bộ Staff
        public async Task<string> QueryStaffAsync(string? query, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(query))
                query = "SELECT * FROM Staff";

            var list = new List<Staff>();

            await using var conn = Conn();
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(query, conn);
            await using var rd = await cmd.ExecuteReaderAsync(ct);

            while (await rd.ReadAsync(ct))
                list.Add(MapStaff(rd));

            return JsonConvert.SerializeObject(list);
        }

        // CREATE — nhận JSON Staff, chèn vào DB, trả JSON record đã tạo
        public async Task<string?> CreateStaffAsync(string staff_json, CancellationToken ct = default)
        {




            const string sql = @"
    INSERT INTO Staff
    (FirstName, LastName, Email, Phone, Address, ContractType, Role,
     StandardHoursPerWeek, StandardPayRate, OvertimePayRate, Password)
    OUTPUT INSERTED.*
    VALUES
    (@firstName, @lastName, @email, @phone, @address, @contractType, @role,
     @standardHoursPerWeek, @standardPayRate, @overtimePayRate, @password);";


            var staff = JsonConvert.DeserializeObject<Staff>(staff_json)
                        ?? throw new ArgumentException("Invalid staff JSON.");
             

            var staff_exist = await CheckStaffFromEmail(staff.Email, ct);
            if (staff_exist !=null)
            {
                throw new ArgumentException("This Email was registed.");
            }






            // (tối thiểu) validate theo schema
            if (string.IsNullOrWhiteSpace(staff.FirstName) || string.IsNullOrWhiteSpace(staff.LastName))
                throw new ArgumentException("FirstName and LastName are required.");

            await using var conn = Conn();
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@firstName", staff.FirstName);
            cmd.Parameters.AddWithValue("@lastName", staff.LastName);
            cmd.Parameters.AddWithValue("@email", (object?)staff.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@phone", (object?)staff.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@address", (object?)staff.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@contractType", (object?)staff.ContractType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@role", (object?)staff.Role ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@standardHoursPerWeek", (object?)staff.StandardHoursPerWeek ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@standardPayRate", (object?)staff.StandardPayRate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@overtimePayRate", (object?)staff.OvertimePayRate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@password", (object?)staff.Password ?? DBNull.Value);

            await using var rd = await cmd.ExecuteReaderAsync(ct);
            var created = await rd.ReadAsync(ct) ? MapStaff(rd) : null;

            return JsonConvert.SerializeObject(created);
        }

        // UPDATE — nhận id + JSON Staff, cập nhật và trả JSON record đã cập nhật
        public async Task<string> UpdateStaffAsync(int id, string staff_json, CancellationToken ct = default)
        {
            const string sql = @"
    UPDATE Staff
    SET FirstName = @firstName,
        LastName  = @lastName,
        Email     = @email,
        Phone     = @phone,
        Address   = @address,
        ContractType = @contractType,
        Role      = @role,
        StandardHoursPerWeek = @standardHoursPerWeek,
        StandardPayRate      = @standardPayRate,
        OvertimePayRate      = @overtimePayRate,
        Password  = @password
    OUTPUT INSERTED.*
    WHERE StaffId = @id;";


            var staff = JsonConvert.DeserializeObject<Staff>(staff_json)
                        ?? throw new ArgumentException("Invalid staff JSON.");





            //var staff_exist = await CheckStaffFromEmail(staff.Email, ct);
            //if (staff_exist != null || staff_exist.StaffId != staff.StaffId)
            //{
            //    throw new ArgumentException("Some one registered this Email before");
            //}







            await using var conn = Conn();
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@firstName", staff.FirstName);
            cmd.Parameters.AddWithValue("@lastName", staff.LastName);
            cmd.Parameters.AddWithValue("@email", (object?)staff.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@phone", (object?)staff.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@address", (object?)staff.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@contractType", (object?)staff.ContractType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@role", (object?)staff.Role ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@standardHoursPerWeek", (object?)staff.StandardHoursPerWeek ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@standardPayRate", (object?)staff.StandardPayRate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@overtimePayRate", (object?)staff.OvertimePayRate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@password", (object?)staff.Password ?? DBNull.Value);

            await using var rd = await cmd.ExecuteReaderAsync(ct);
            var updated = await rd.ReadAsync(ct) ? MapStaff(rd) : null;

            return JsonConvert.SerializeObject(updated);
        }

        // DELETE — nhận id, xóa và trả JSON record đã xóa (hoặc null nếu không tìm thấy)
        public async Task<string> DeleteStaffAsync(int id, CancellationToken ct = default)
        {
            const string sql = @"DELETE FROM Staff OUTPUT DELETED.* WHERE StaffId = @id;";

            await using var conn = Conn();
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await using var rd = await cmd.ExecuteReaderAsync(ct);
            var deleted = await rd.ReadAsync(ct) ? MapStaff(rd) : null;

            return JsonConvert.SerializeObject(deleted);
        }






 

 
        // LOGIN — truyền JSON { "Email": "...", "Password": "..." }
        public async Task<string?> LoginAsync(string login_json, HttpContext httpContext, CancellationToken ct = default)
        {
            // 1) Parse input
            var login = JsonConvert.DeserializeObject<Dictionary<string, string>>(login_json);
            if (login is null || !login.TryGetValue("Email", out var email) || !login.TryGetValue("Password", out var password))
                throw new ArgumentException("Login JSON must include Email and Password");

            email ??= string.Empty;
            password ??= string.Empty;

            // 2) Gọi lại QueryStaffAsync để tìm nhân viên khớp
            static string Esc(string s) => s.Replace("'", "''"); // tối thiểu tránh vỡ query
            var sql = $@"SELECT * FROM Staff WHERE Email = '{Esc(email)}' AND Password = '{Esc(password)}'";
            string jsonArray = await QueryStaffAsync(sql, ct);

            var staffs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Staff>>(jsonArray) ?? new List<Staff>();

            Staff staff = staffs.Count == 1 ? staffs[0] : null;
                
                

            // 3) Ghi History
            var history = new History
            {
                Timestamp = DateTime.UtcNow,
                Actor = email,              // tên đối tượng là email
                                // nếu cần IP, bạn lấy từ HttpContext và truyền xuống
                Action = "Login",
                Result = staff != null ? "Succeed" : "Failed",
                Details = staff != null ? null : "Wrong username or password"
            }; 

            string create_his =   await _hist.CreateHistoryAsync(JsonConvert.SerializeObject(history), httpContext, ct);


            if (!string.IsNullOrEmpty(create_his))
            {

                if (history.Result == "Succeed" && staff.Role == "Admin")
                {
                    return JsonConvert.SerializeObject(staff); 
                } 
            } 

            return null;
          

        }




        // CHECK — kiểm tra Staff theo Email
        public async Task<Staff?> CheckStaffFromEmail(string email, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            // Escape đơn giản tránh SQL Injection (tối thiểu)
            static string Esc(string s) => s.Replace("'", "''");
            string sql = $@"SELECT * FROM Staff WHERE Email = '{Esc(email)}'";

            await using var conn = Conn();
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            await using var rd = await cmd.ExecuteReaderAsync(ct);

            Staff? staff = null;
            if (await rd.ReadAsync(ct))
                staff = MapStaff(rd);

            return staff; // trả về Staff hoặc null
        }












    }







}

