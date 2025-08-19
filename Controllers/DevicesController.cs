using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RestfulAPI_FarmTimeManagement.Models;

namespace RestfulAPI_FarmTimeManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly string _connStr;
        public DevicesController(IConfiguration cfg)
        {
            _connStr = cfg.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");
        }
        private SqlConnection Conn() => new SqlConnection(_connStr);

        private static Device Map(SqlDataReader rd) => new Device
        {
            DeviceId = rd.GetInt32(0),
            Location = rd.IsDBNull(1) ? null : rd.GetString(1),
            Type = rd.IsDBNull(2) ? null : rd.GetString(2),
            Status = rd.IsDBNull(3) ? null : rd.GetString(3),
        };

        // GET: api/devices?query=
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Device>>> GetAll([FromQuery] string? query)
        {
            var list = new List<Device>();
            using var c = Conn(); await c.OpenAsync();

            const string sql = """
                SELECT deviceId, location, type, status
                FROM Device
                WHERE (@q IS NULL OR (location LIKE '%'+@q+'%' OR type LIKE '%'+@q+'%' OR status LIKE '%'+@q+'%'))
                ORDER BY deviceId DESC;
                """;
            using var cmd = new SqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@q", (object?)query ?? DBNull.Value);

            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync()) list.Add(Map(rd));
            return Ok(list);
        }

        // GET: api/devices/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Device>> Get(int id)
        {
            using var c = Conn(); await c.OpenAsync();
            const string sql = "SELECT deviceId, location, type, status FROM Device WHERE deviceId=@id;";
            using var cmd = new SqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@id", id);

            using var rd = await cmd.ExecuteReaderAsync();
            if (!await rd.ReadAsync()) return NotFound();
            return Ok(Map(rd));
        }

        // POST: api/devices
        [HttpPost]
        public async Task<ActionResult<Device>> Create([FromBody] Device dto)
        {
            using var c = Conn(); await c.OpenAsync();
            const string sql = """
                INSERT INTO Device(location, type, status)
                OUTPUT INSERTED.deviceId
                VALUES (@loc, @type, @status);
                """;
            using var cmd = new SqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@loc", (object?)dto.Location ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@type", (object?)dto.Type ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@status", (object?)dto.Status ?? DBNull.Value);

            dto.DeviceId = (int)await cmd.ExecuteScalarAsync();
            return CreatedAtAction(nameof(Get), new { id = dto.DeviceId }, dto);
        }

        // PUT: api/devices/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Device dto)
        {
            using var c = Conn(); await c.OpenAsync();
            const string sql = """
                UPDATE Device SET location=@loc, type=@type, status=@status
                WHERE deviceId=@id;
                """;
            using var cmd = new SqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@loc", (object?)dto.Location ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@type", (object?)dto.Type ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@status", (object?)dto.Status ?? DBNull.Value);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows == 0 ? NotFound() : NoContent();
        }

        // DELETE: api/devices/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            using var c = Conn(); await c.OpenAsync();
            const string sql = "DELETE FROM Device WHERE deviceId=@id;";
            using var cmd = new SqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@id", id);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows == 0 ? NotFound() : NoContent();
        }
    }
}
