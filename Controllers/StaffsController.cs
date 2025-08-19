using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RestfulAPI_FarmTimeManagement.Models; // Đổi "MyApi" thành namespace thực tế của bạn
using System.Data;

namespace RestfulAPI_FarmTimeManagement.Controllers // Đổi "MyApi" thành namespace thực tế của bạn
{
    [ApiController]
    [Route("api/[controller]")]
    public class StaffsController : ControllerBase
    {
        private readonly string _connStr;

        public StaffsController(IConfiguration cfg)
        {
            _connStr = cfg.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");
        }

        private SqlConnection Conn() => new SqlConnection(_connStr);


        private static Staff MapStaff(SqlDataReader rd) => new Staff
        {
            StaffId = rd.GetInt32(0),
            FirstName = rd.GetString(1),
            LastName = rd.GetString(2),
            Email = rd.IsDBNull(3) ? null : rd.GetString(3),
            Phone = rd.IsDBNull(4) ? null : rd.GetString(4),
            Address = rd.IsDBNull(5) ? null : rd.GetString(5),
            ContractType = rd.IsDBNull(6) ? null : rd.GetString(6),
            IsActive = rd.IsDBNull(7) ? (bool?)null : rd.GetBoolean(7),
            Role = rd.IsDBNull(8) ? null : rd.GetString(8),
            StandardHoursPerWeek = rd.IsDBNull(9) ? (decimal?)null : rd.GetDecimal(9),
            StandardPayRate = rd.IsDBNull(10) ? (decimal?)null : rd.GetDecimal(10),
            OvertimePayRate = rd.IsDBNull(11) ? (decimal?)null : rd.GetDecimal(11)
        };


         


      
        // GET: api/staffs?query=
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Staff>>> GetAll([FromQuery] string? query)
        {
            var list = new List<Staff>();

            using var c = Conn();
            await c.OpenAsync();

            var sql = """
        SELECT staffId, firstName, lastName, email, phone, address, contractType, isActive, role,
               standardHoursPerWeek, standardPayRate, overtimePayRate
        FROM Staff
        WHERE (@q IS NULL OR 
               (firstName + ' ' + lastName LIKE '%' + @q + '%' 
                OR email LIKE '%' + @q + '%' 
                OR role LIKE '%' + @q + '%'))
        ORDER BY lastName, firstName;
        """;

            using var cmd = new SqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@q", (object?)query ?? DBNull.Value);

            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
                list.Add(MapStaff(rd));

            return Ok(list);
        }

        // GET: api/staffs/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Staff>> Get(int id)
        {
            using var c = Conn();
            await c.OpenAsync();

            const string sql = """
                SELECT staffId, firstName, lastName, email, phone, address, contractType, isActive, role,
                       standardHoursPerWeek, standardPayRate, overtimePayRate
                FROM Staff WHERE staffId = @id;
                """;

            using var cmd = new SqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@id", id);

            using var rd = await cmd.ExecuteReaderAsync();
            if (!await rd.ReadAsync()) return NotFound();

            return Ok(MapStaff(rd));
        }

        // POST: api/staffs
        [HttpPost]
        public async Task<ActionResult<Staff>> Create([FromBody] Staff dto)
        {
            if (dto is null) return BadRequest();

            using var c = Conn();
            await c.OpenAsync();

            const string sql = """
                INSERT INTO Staff (firstName, lastName, email, phone, address, contractType, isActive, role,
                                   standardHoursPerWeek, standardPayRate, overtimePayRate)
                OUTPUT INSERTED.staffId
                VALUES (@f, @l, @e, @p, @a, @ct, @ia, @r, @h, @sp, @op);
                """;

            using var cmd = new SqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@f", dto.FirstName);
            cmd.Parameters.AddWithValue("@l", dto.LastName);
            cmd.Parameters.AddWithValue("@e", (object?)dto.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@p", (object?)dto.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@a", (object?)dto.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ct", (object?)dto.ContractType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ia", (object?)dto.IsActive ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@r", (object?)dto.Role ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@h", (object?)dto.StandardHoursPerWeek ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@sp", (object?)dto.StandardPayRate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@op", (object?)dto.OvertimePayRate ?? DBNull.Value);

            dto.StaffId = (int)await cmd.ExecuteScalarAsync();
            return CreatedAtAction(nameof(Get), new { id = dto.StaffId }, dto);
        }

        // PUT: api/staffs/5  (không cho đổi staffId)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Staff dto)
        {
            if (dto is null) return BadRequest();

            using var c = Conn();
            await c.OpenAsync();

            const string sql = """
                UPDATE Staff
                SET firstName = @f,
                    lastName = @l,
                    email = @e,
                    phone = @p,
                    address = @a,
                    contractType = @ct,
                    isActive = @ia,
                    role = @r,
                    standardHoursPerWeek = @h,
                    standardPayRate = @sp,
                    overtimePayRate = @op
                WHERE staffId = @id;
                """;

            using var cmd = new SqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@f", dto.FirstName);
            cmd.Parameters.AddWithValue("@l", dto.LastName);
            cmd.Parameters.AddWithValue("@e", (object?)dto.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@p", (object?)dto.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@a", (object?)dto.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ct", (object?)dto.ContractType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ia", (object?)dto.IsActive ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@r", (object?)dto.Role ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@h", (object?)dto.StandardHoursPerWeek ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@sp", (object?)dto.StandardPayRate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@op", (object?)dto.OvertimePayRate ?? DBNull.Value);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows == 0 ? NotFound() : NoContent();
        }

        // DELETE: api/staffs/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            using var c = Conn();
            await c.OpenAsync();

            const string sql = "DELETE FROM Staff WHERE staffId = @id;";

            using var cmd = new SqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@id", id);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows == 0 ? NotFound() : NoContent();
        }
    }
}
