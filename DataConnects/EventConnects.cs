using Microsoft.Data.SqlClient;
using RestfulAPI_FarmTimeManagement.Models;
using System.Data;

namespace RestfulAPI_FarmTimeManagement.DataConnects
{
    public class EventConnects
    {
        private SqlConnection Conn() => new SqlConnection(Config._connString);

        private static Event Map(SqlDataReader rd)
        {
            int iEventId = rd.GetOrdinal("EventId");
            int iTimestamp = rd.GetOrdinal("Timestamp");
            int iStaffId = rd.GetOrdinal("StaffId");
            int iDeviceId = rd.GetOrdinal("DeviceId");
            // DB column is EventType (per current SQL), model property is EventType
            int iEventType = rd.GetOrdinal("EventType");
            int iReason = rd.GetOrdinal("Reason");

            return new Event
            {
                EventId = rd.GetInt32(iEventId),
                Timestamp = rd.GetDateTime(iTimestamp),
                StaffId = rd.GetInt32(iStaffId),
                DeviceId = rd.IsDBNull(iDeviceId) ? (int?)null : rd.GetInt32(iDeviceId),
                EventType = rd.GetString(iEventType), // map DB 'EventType' -> model 'EventType'
                Reason = rd.IsDBNull(iReason) ? null : rd.GetString(iReason)
            };
        }

        // QUERY
        public async Task<List<Event>> QueryEvent(string? querySql = null)
        {
            // Note: select EventType to match DB; mapper fills EventType
            querySql ??= "SELECT EventId, Timestamp, StaffId, DeviceId, EventType, Reason FROM Event";
            var list = new List<Event>();

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(querySql, conn);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
                list.Add(Map(rd));
            return list;
        }

        // CREATE
        public async Task<Event?> CreateEvent(Event item)
        {
            const string sql = @"
INSERT INTO Event (Timestamp, StaffId, DeviceId, EventType, Reason)
OUTPUT INSERTED.EventId, INSERTED.Timestamp, INSERTED.StaffId, INSERTED.DeviceId, INSERTED.EventType, INSERTED.Reason
VALUES (@Timestamp, @StaffId, @DeviceId, @EventType, @Reason);";

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Timestamp", item.Timestamp);
            cmd.Parameters.AddWithValue("@StaffId", item.StaffId);
            if (item.DeviceId.HasValue)
                cmd.Parameters.AddWithValue("@DeviceId", item.DeviceId.Value);
            else
                cmd.Parameters.Add("@DeviceId", SqlDbType.Int).Value = DBNull.Value;
            cmd.Parameters.AddWithValue("@EventType", item.EventType); // pass model EventType into DB column EventType
            if (item.Reason is not null)
                cmd.Parameters.AddWithValue("@Reason", item.Reason);
            else
                cmd.Parameters.Add("@Reason", SqlDbType.NVarChar, 255).Value = DBNull.Value;

            await using var rd = await cmd.ExecuteReaderAsync();
            return await rd.ReadAsync() ? Map(rd) : null;
        }

        // UPDATE
        public async Task<Event?> UpdateEvent(int eventId, Event item)
        {
            const string sql = @"
UPDATE Event
SET Timestamp = @Timestamp,
    StaffId   = @StaffId,
    DeviceId  = @DeviceId,
    EventType  = @EventType,
    Reason    = @Reason
OUTPUT INSERTED.EventId, INSERTED.Timestamp, INSERTED.StaffId, INSERTED.DeviceId, INSERTED.EventType, INSERTED.Reason
WHERE EventId = @EventId;";

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@EventId", eventId);
            cmd.Parameters.AddWithValue("@Timestamp", item.Timestamp);
            cmd.Parameters.AddWithValue("@StaffId", item.StaffId);
            if (item.DeviceId.HasValue)
                cmd.Parameters.AddWithValue("@DeviceId", item.DeviceId.Value);
            else
                cmd.Parameters.Add("@DeviceId", SqlDbType.Int).Value = DBNull.Value;
            cmd.Parameters.AddWithValue("@EventType", item.EventType);
            if (item.Reason is not null)
                cmd.Parameters.AddWithValue("@Reason", item.Reason);
            else
                cmd.Parameters.Add("@Reason", SqlDbType.NVarChar, 255).Value = DBNull.Value;

            await using var rd = await cmd.ExecuteReaderAsync();
            return await rd.ReadAsync() ? Map(rd) : null;
        }

        // DELETE
        public async Task<Event?> DeleteEvent(int eventId)
        {
            const string sql = @"
DELETE FROM Event
OUTPUT DELETED.EventId, DELETED.Timestamp, DELETED.StaffId, DELETED.DeviceId, DELETED.EventType, DELETED.Reason
WHERE EventId = @EventId;";

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@EventId", eventId);

            await using var rd = await cmd.ExecuteReaderAsync();
            return await rd.ReadAsync() ? Map(rd) : null;
        }
    }
}
