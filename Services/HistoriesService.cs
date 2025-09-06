using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using RestfulAPI_FarmTimeManagement.Models;
using System.Net;
 

namespace RestfulAPI_FarmTimeManagement.Services
{
    public class HistoriesService
    {
        private readonly string _connStr;
        public HistoriesService(IConfiguration cfg)
        {
            _connStr = cfg.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");
        }

     

        private SqlConnection Conn() => new SqlConnection(_connStr);

        

        // ===== Helpers & Mapper =====
        private static T? GetNullable<T>(SqlDataReader r, int i) => r.IsDBNull(i) ? default : r.GetFieldValue<T>(i);

        private static History MapHistory(SqlDataReader rd)
        {
            int iId = rd.GetOrdinal("HistoryId");
            int iTs = rd.GetOrdinal("Timestamp");
            int iActor = rd.GetOrdinal("Actor");
            int iIp = rd.GetOrdinal("Ip");
            int iAct = rd.GetOrdinal("Action");
            int iRes = rd.GetOrdinal("Result");
            int iDet = rd.GetOrdinal("Details");

            return new History
            {
                HistoryId = rd.GetInt32(iId),
                Timestamp = rd.GetDateTime(iTs),
                Actor = GetNullable<string>(rd, iActor),
                Ip = GetNullable<string>(rd, iIp),
                Action = GetNullable<string>(rd, iAct),
                Result = GetNullable<string>(rd, iRes),
                Details = GetNullable<string>(rd, iDet)
            };
        }

        // ===================== QUERY =====================
        // Nếu query null/empty -> SELECT * FROM History
        public async Task<string> QueryHistoriesAsync(string? query, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(query))
                query = "SELECT * FROM History ORDER BY [Timestamp] DESC, HistoryId DESC";

            var list = new List<History>();

            await using var conn = Conn();
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(query, conn);
            await using var rd = await cmd.ExecuteReaderAsync(ct);

            while (await rd.ReadAsync(ct))
                list.Add(MapHistory(rd));

            return JsonConvert.SerializeObject(list);
        }

        // ===================== CREATE =====================
        // Nhận JSON History, chèn và trả lại JSON record đã tạo
        // Lưu ý: Timestamp là NOT NULL trong DB -> nếu không truyền, mặc định UtcNow
        public async Task<string> CreateHistoryAsync(string history_json, HttpContext ctx,  CancellationToken ct = default)
        {
            const string sql = @"
INSERT INTO History ([Timestamp], Actor, Ip, Action, Result, Details)
OUTPUT INSERTED.*
VALUES (@ts, @actor, @ip, @action, @result, @details);";

            var h = JsonConvert.DeserializeObject<History>(history_json)
                    ?? throw new ArgumentException("Invalid history JSON.");

             

            if (h.Timestamp == default) h.Timestamp = DateTime.UtcNow;



            h.Ip = GetClientIp(ctx);





            await using var conn = Conn();
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@ts", h.Timestamp);
            cmd.Parameters.AddWithValue("@actor", (object?)h.Actor ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ip", (object?)h.Ip ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@action", (object?)h.Action ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@result", (object?)h.Result ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@details", (object?)h.Details ?? DBNull.Value);

            await using var rd = await cmd.ExecuteReaderAsync(ct);
            var created = await rd.ReadAsync(ct) ? MapHistory(rd) : null;

            return JsonConvert.SerializeObject(created);
        }

        // ===================== DELETE =====================
        // Xoá theo HistoryId, trả lại JSON record đã xoá (hoặc null nếu không tồn tại)
        public async Task<string> DeleteHistoryAsync(int id, CancellationToken ct = default)
        {
            const string sql = @"DELETE FROM History OUTPUT DELETED.* WHERE HistoryId = @id;";

            await using var conn = Conn();
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await using var rd = await cmd.ExecuteReaderAsync(ct);
            var deleted = await rd.ReadAsync(ct) ? MapHistory(rd) : null;

            return JsonConvert.SerializeObject(deleted);
        }



         




        private static string? GetClientIp(HttpContext ctx)
        {
            // Ưu tiên header khi đứng sau proxy/CDN
            var xRealIp = ctx.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(xRealIp))
                return xRealIp.Split(',')[0].Trim();

            var xff = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(xff))
                return xff.Split(',')[0].Trim();

            // Lấy IP trực tiếp từ kết nối
            var addr = ctx.Connection.RemoteIpAddress;
            if (addr == null) return null;

            // Nếu là IPv4-mapped IPv6 thì map về IPv4
            if (addr.IsIPv4MappedToIPv6)
                addr = addr.MapToIPv4();

            // Nếu là loopback (::1 hoặc 127.0.0.1) -> tuỳ chọn:
            //   a) Trả "127.0.0.1" để dễ đọc
            //   b) Hoặc trả null để khỏi lưu vào DB
            if (IPAddress.IsLoopback(addr))
                return "127.0.0.1"; // hoặc: return null;

            return addr.ToString();
        }



    }
}
