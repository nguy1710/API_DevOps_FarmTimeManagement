using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using RestfulAPI_FarmTimeManagement.Models;
using System.Net;
 

namespace RestfulAPI_FarmTimeManagement.DataConnects
{
    public class HistoryConnects
    {
        
     

        private SqlConnection Conn() => new SqlConnection(Config._connString); 

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

 
        public async Task<List<History>> QueryHistory(string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
                query = "SELECT * FROM History ORDER BY [Timestamp] DESC, HistoryId DESC";

            var list = new List<History>();

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(query, conn);
            await using var rd = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
                list.Add(MapHistory(rd));

            return list;
        }

      
        public async Task<History> CreateHistory(History history)
        {

            history.Timestamp = DateTime.Now;

            const string sql = @"
INSERT INTO History ([Timestamp], Actor, Ip, Action, Result, Details)
OUTPUT INSERTED.*
VALUES (@ts, @actor, @ip, @action, @result, @details);"; 
            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn); 
            cmd.Parameters.AddWithValue("@ts", history.Timestamp);
            cmd.Parameters.AddWithValue("@actor", (object?)history.Actor ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ip", (object?)history.Ip ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@action", (object?)history.Action ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@result", (object?)history.Result ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@details", (object?)history.Details ?? DBNull.Value);

            await using var rd = await cmd.ExecuteReaderAsync();
            var created = await rd.ReadAsync() ? MapHistory(rd) : null;

            return created;
        }




        public async Task<History> DeleteHistory(int id)
        {
            const string sql = @"DELETE FROM History OUTPUT DELETED.* WHERE HistoryId = @id;";

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await using var rd = await cmd.ExecuteReaderAsync();
            var deleted = await rd.ReadAsync() ? MapHistory(rd) : null;

            return deleted;
        }

         

 


    }
}
