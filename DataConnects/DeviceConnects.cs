using Microsoft.Data.SqlClient;
using RestfulAPI_FarmTimeManagement.Models;
using System.Data;

namespace RestfulAPI_FarmTimeManagement.DataConnects
{
    public class DeviceConnects
    {
        private SqlConnection Conn() => new SqlConnection(Config._connString);

        private static Device Map(SqlDataReader rd)
        {
            int iDeviceId = rd.GetOrdinal("DeviceId");
            int iLocation = rd.GetOrdinal("Location");
            int iType = rd.GetOrdinal("Type");
            int iStatus = rd.GetOrdinal("Status");

            return new Device
            {
                DeviceId = rd.GetInt32(iDeviceId),
                Location = rd.GetString(iLocation),
                Type = rd.GetString(iType),
                Status = rd.GetString(iStatus)
            };
        }

        // QUERY
        public async Task<List<Device>> QueryDevice(string? querySql = null)
        {
            querySql ??= "SELECT * FROM Device";
            var list = new List<Device>();

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(querySql, conn);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
                list.Add(Map(rd));
            return list;
        }

        // CREATE
        public async Task<Device?> CreateDevice(Device item)
        {
            const string sql = @"
INSERT INTO Device (Location, Type, Status)
OUTPUT INSERTED.*
VALUES (@Location, @Type, @Status);";

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Location", item.Location);
            cmd.Parameters.AddWithValue("@Type", item.Type);
            cmd.Parameters.AddWithValue("@Status", item.Status);

            await using var rd = await cmd.ExecuteReaderAsync();
            return await rd.ReadAsync() ? Map(rd) : null;
        }

        // UPDATE
        public async Task<Device?> UpdateDevice(int deviceId, Device item)
        {
            const string sql = @"
UPDATE Device
SET Location = @Location,
    Type     = @Type,
    Status   = @Status
OUTPUT INSERTED.*
WHERE DeviceId = @DeviceId;";

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@DeviceId", deviceId);
            cmd.Parameters.AddWithValue("@Location", item.Location);
            cmd.Parameters.AddWithValue("@Type", item.Type);
            cmd.Parameters.AddWithValue("@Status", item.Status);

            await using var rd = await cmd.ExecuteReaderAsync();
            return await rd.ReadAsync() ? Map(rd) : null;
        }

        // DELETE
        public async Task<Device?> DeleteDevice(int deviceId)
        {
            const string sql = @"DELETE FROM Device OUTPUT DELETED.* WHERE DeviceId = @DeviceId;";

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@DeviceId", deviceId);

            await using var rd = await cmd.ExecuteReaderAsync();
            return await rd.ReadAsync() ? Map(rd) : null;
        }
    }
}
