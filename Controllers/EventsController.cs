using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RestfulAPI_FarmTimeManagement.Models;

namespace RestfulAPI_FarmTimeManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly string _connStr;
        public EventsController(IConfiguration cfg)
        {
            _connStr = cfg.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");
        }
        private SqlConnection Conn() => new SqlConnection(_connStr);

        private static Event Map(SqlDataReader rd) => new Event
        {
            EventId = rd.GetInt64(0),
            StaffId = rd.GetInt32(1),
            TimeStamp = rd.GetDateTime(2),
            EventType = rd.IsDBNull(3) ? null : rd.GetString(3),
            Reason = rd.IsDBNull(4) ? null : rd.GetString(4),
            DeviceId = rd.IsDBNull(5) ? (int?)null : rd.GetInt32(5),
            AdminId = rd.IsDBNull(6) ? (int?)null : rd.GetInt32(6),
        };

        // GET: api/events?staffId=&deviceId=&adminId=&type=&from=&to=
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetAll(
            [FromQuery] int? staffId,
            [FromQuery] int? deviceId,
            [FromQuery] int? adminId,
            [FromQuery] string? type,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var list = new List<Event>();
            using var c = Conn(); await c.OpenAsync();

            const string sql = """
                SELECT eventId, staffId, timeStamp, eventType, reason, deviceId, adminId
                FROM Events
                WHERE (@staffId IS NULL OR staffId=@staffId)
                  AND (@deviceId IS NULL OR deviceId=@deviceId)
                  AND (@adminId IS NULL OR adminId=@adminId)
                  AND (@type IS NULL OR eventType=@type)
                  AND (@from IS NULL OR timeStamp >= @from)
                  AND (@to   IS NULL OR timeStamp <  @to)
                ORDER BY timeStamp DESC, eventId DESC;
                """;
            using var cmd = new SqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@staffId", (object?)staffId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@deviceId", (object?)deviceId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@adminId", (object?)adminId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@type", (object?)type ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@from", (object?)from ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@to", (object?)to ?? DBNull.Value);

            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync()) list.Add(Map(rd));
            return Ok(list);
        }

        // GET: api/events/123
        [HttpGet("{id:long}")]
        public async Task<ActionResult<Event>> Get(long id)
        {
            using var c = Conn(); await c.OpenAsync();
            const string sql = """
                SELECT eventId, staffId, timeStamp, eventType, reason, deviceId, adminId
                FROM Events WHERE eventId=@id;
                """;
            using var cmd = new SqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@id", id);

            using var rd = await cmd.ExecuteReaderAsync();
            if (!await rd.ReadAsync()) return NotFound();
            return Ok(Map(rd));
        }

        // POST: api/events
        [HttpPost]
        public async Task<ActionResult<Event>> Create([FromBody] Event dto)
        {
            if (dto.TimeStamp == default) dto.TimeStamp = DateTime.UtcNow; // default nếu client không gửi

            using var c = Conn(); await c.OpenAsync();
            const string sql = """
                INSERT INTO Events(staffId, timeStamp, eventType, reason, deviceId, adminId)
                OUTPUT INSERTED.eventId
                VALUES (@staffId, @ts, @type, @reason, @deviceId, @adminId);
                """;
            using var cmd = new SqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@staffId", dto.StaffId);
            cmd.Parameters.AddWithValue("@ts", dto.TimeStamp);
            cmd.Parameters.AddWithValue("@type", (object?)dto.EventType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@reason", (object?)dto.Reason ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@deviceId", (object?)dto.DeviceId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@adminId", (object?)dto.AdminId ?? DBNull.Value);

            dto.EventId = (long)await cmd.ExecuteScalarAsync();
            return CreatedAtAction(nameof(Get), new { id = dto.EventId }, dto);
        }

        // PUT: api/events/123
        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] Event dto)
        {
            using var c = Conn(); await c.OpenAsync();
            const string sql = """
                UPDATE Events
                SET staffId=@staffId, timeStamp=@ts, eventType=@type, reason=@reason, deviceId=@deviceId, adminId=@adminId
                WHERE eventId=@id;
                """;
            using var cmd = new SqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@staffId", dto.StaffId);
            cmd.Parameters.AddWithValue("@ts", dto.TimeStamp == default ? DateTime.UtcNow : dto.TimeStamp);
            cmd.Parameters.AddWithValue("@type", (object?)dto.EventType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@reason", (object?)dto.Reason ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@deviceId", (object?)dto.DeviceId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@adminId", (object?)dto.AdminId ?? DBNull.Value);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows == 0 ? NotFound() : NoContent();
        }

        // DELETE: api/events/123
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            using var c = Conn(); await c.OpenAsync();
            const string sql = "DELETE FROM Events WHERE eventId=@id;";
            using var cmd = new SqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@id", id);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows == 0 ? NotFound() : NoContent();
        }
    }
}
