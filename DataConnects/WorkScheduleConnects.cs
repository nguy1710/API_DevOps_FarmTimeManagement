using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using RestfulAPI_FarmTimeManagement.Models;
using System.Data;

namespace RestfulAPI_FarmTimeManagement.DataConnects
{
    public class WorkScheduleConnects
    {
        private SqlConnection Conn() => new SqlConnection(Config._connString);

        private static T? GetNullable<T>(SqlDataReader r, int i) => r.IsDBNull(i) ? default : r.GetFieldValue<T>(i);

        private static WorkSchedule MapWorkSchedule(SqlDataReader rd)
        {
            int iScheduleId = rd.GetOrdinal("ScheduleId");
            int iStaffId = rd.GetOrdinal("StaffId");
            int iStart = rd.GetOrdinal("StartTime");
            int iEnd = rd.GetOrdinal("EndTime");
            int iHours = rd.GetOrdinal("ScheduleHours");

            return new WorkSchedule
            {
                ScheduleId = rd.GetInt32(iScheduleId),
                StaffId = rd.GetInt32(iStaffId),
                StartTime = rd.GetDateTime(iStart),
                EndTime = rd.GetDateTime(iEnd),
                ScheduleHours = rd.GetInt32(iHours)
            };
        }

        #region CRUD WORKSCHEDULE

        // QUERY
        public async Task<List<WorkSchedule>> QueryWorkSchedule(string query_stringSQL)
        {
            if (string.IsNullOrWhiteSpace(query_stringSQL))
                query_stringSQL = "SELECT * FROM WorkSchedule";

            var list = new List<WorkSchedule>();

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(query_stringSQL, conn);
            await using var rd = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
                list.Add(MapWorkSchedule(rd));

            return list;
        }

        // CREATE
        public async Task<WorkSchedule?> CreateWorkSchedule(WorkSchedule ws)
        {
            const string sql = @"
INSERT INTO WorkSchedule
(StaffId, StartTime, EndTime, ScheduleHours)
OUTPUT INSERTED.*
VALUES
(@StaffId, @StartTime, @EndTime, @ScheduleHours);";

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@StaffId", ws.StaffId);
            cmd.Parameters.AddWithValue("@StartTime", ws.StartTime);   // DATETIME2(0)
            cmd.Parameters.AddWithValue("@EndTime", ws.EndTime);       // DATETIME2(0)
            cmd.Parameters.AddWithValue("@ScheduleHours", ws.ScheduleHours);

            await using var rd = await cmd.ExecuteReaderAsync();
            var created = await rd.ReadAsync() ? MapWorkSchedule(rd) : null;
            return created;
        }

        // UPDATE
        public async Task<WorkSchedule?> UpdateWorkSchedule(int id, WorkSchedule ws)
        {
            const string sql = @"
UPDATE WorkSchedule
SET StaffId       = @StaffId,
    StartTime     = @StartTime,
    EndTime       = @EndTime,
    ScheduleHours = @ScheduleHours
OUTPUT INSERTED.*
WHERE ScheduleId = @ScheduleId;";

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@ScheduleId", id);
            cmd.Parameters.AddWithValue("@StaffId", ws.StaffId);
            cmd.Parameters.AddWithValue("@StartTime", ws.StartTime);
            cmd.Parameters.AddWithValue("@EndTime", ws.EndTime);
            cmd.Parameters.AddWithValue("@ScheduleHours", ws.ScheduleHours);

            await using var rd = await cmd.ExecuteReaderAsync();
            var updated = await rd.ReadAsync() ? MapWorkSchedule(rd) : null;
            return updated;
        }

        // DELETE
        public async Task<WorkSchedule?> DeleteWorkSchedule(int id)
        {
            const string sql = @"DELETE FROM WorkSchedule OUTPUT DELETED.* WHERE ScheduleId = @id;";

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await using var rd = await cmd.ExecuteReaderAsync();
            var deleted = await rd.ReadAsync() ? MapWorkSchedule(rd) : null;
            return deleted;
        }

        #endregion
    }
}
