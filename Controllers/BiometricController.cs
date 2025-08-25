using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RestfulAPI_FarmTimeManagement.Models;

namespace RestfulAPI_FarmTimeManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BiometricController : ControllerBase
    {
        private readonly string _connStr;
        public BiometricController(IConfiguration cfg)
        {
            _connStr = cfg.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");
        }
        private SqlConnection Conn() => new SqlConnection(_connStr);

        private static BiometricData Map(SqlDataReader rd) => new BiometricData
        {
            BiometricId = rd.GetInt32(0),
            StaffId = rd.GetInt32(1),
            Template = rd.GetString(2) // đọc từ cột biometricTemplate (nvarchar)
        };

        // GET: api/biometric?staffId=
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BiometricData>>> GetAll([FromQuery] int? staffId)
        {
            var list = new List<BiometricData>();
            using var c = Conn();
            await c.OpenAsync();

            const string sql = """
                SELECT biometricId, staffId, biometricTemplate
                FROM BiometricData
                WHERE (@staffId IS NULL OR staffId = @staffId)
                ORDER BY biometricId DESC;
                """;

            using var cmd = new SqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@staffId", (object?)staffId ?? DBNull.Value);

            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync()) list.Add(Map(rd));
            return Ok(list);
        }

 


        // GET: api/biometric/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<BiometricData>> Get(int id)
        {
            using var c = Conn();
            await c.OpenAsync();
            const string sql = """
                SELECT biometricId, staffId, biometricTemplate
                FROM BiometricData
                WHERE biometricId = @id;
                """;
            using var cmd = new SqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@id", id);

            using var rd = await cmd.ExecuteReaderAsync();
            if (!await rd.ReadAsync()) return NotFound();
            return Ok(Map(rd));
        }

        // POST: api/biometric
        [HttpPost]
        public async Task<ActionResult<BiometricData>> Create([FromBody] BiometricData dto)
        {
            if (dto is null) return BadRequest();
            if (dto.StaffId <= 0 || string.IsNullOrWhiteSpace(dto.Template))
                return BadRequest("staffId and biometricTemplate are required");

            using var c = Conn();
            await c.OpenAsync();

            const string sql = """
                INSERT INTO BiometricData (staffId, biometricTemplate)
                OUTPUT INSERTED.biometricId, INSERTED.staffId, INSERTED.biometricTemplate
                VALUES (@staffId, @template);
                """;

            using var cmd = new SqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@staffId", dto.StaffId);
            cmd.Parameters.AddWithValue("@template", dto.Template);

            using var rd = await cmd.ExecuteReaderAsync();
            await rd.ReadAsync();
            var created = Map(rd);

            return CreatedAtAction(nameof(Get), new { id = created.BiometricId }, created);
        }

        // PUT: api/biometric/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] BiometricData dto)
        {
            if (dto is null) return BadRequest();

            using var c = Conn();
            await c.OpenAsync();

            const string sql = """
                UPDATE BiometricData
                SET staffId = @staffId,
                    biometricTemplate = @template
                WHERE biometricId = @id;
                """;

            using var cmd = new SqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@staffId", dto.StaffId);
            cmd.Parameters.AddWithValue("@template", dto.Template);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows == 0 ? NotFound() : NoContent();
        }

        // DELETE: api/biometric/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            using var c = Conn();
            await c.OpenAsync();
            const string sql = "DELETE FROM BiometricData WHERE biometricId = @id;";
            using var cmd = new SqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@id", id);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows == 0 ? NotFound() : NoContent();
        }
    }
}
