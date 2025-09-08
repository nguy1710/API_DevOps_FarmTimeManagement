using Microsoft.Data.SqlClient;
using RestfulAPI_FarmTimeManagement.Models;
using System.Data;

namespace RestfulAPI_FarmTimeManagement.DataConnects
{
    public class BiometricConnects
    {
        private SqlConnection Conn() => new SqlConnection(Config._connString);

        private static Biometric Map(SqlDataReader rd)
        {
            int iBiometricId = rd.GetOrdinal("BiometricId");
            int iStaffId = rd.GetOrdinal("StaffId");
            int iType = rd.GetOrdinal("Type");
            int iData = rd.GetOrdinal("Data");

            return new Biometric
            {
                BiometricId = rd.GetInt32(iBiometricId),
                StaffId = rd.GetInt32(iStaffId),
                Type = rd.GetString(iType),
                Data = rd.GetString(iData)
            };
        }

        // QUERY
        public async Task<List<Biometric>> QueryBiometric(string? querySql = null)
        {
            querySql ??= "SELECT * FROM Biometric";
            var list = new List<Biometric>();

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(querySql, conn);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
                list.Add(Map(rd));
            return list;
        }

        // CREATE
        public async Task<Biometric?> CreateBiometric(Biometric item)
        {
            const string sql = @"
INSERT INTO Biometric (StaffId, Type, Data)
OUTPUT INSERTED.*
VALUES (@StaffId, @Type, @Data);";

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@StaffId", item.StaffId);
            cmd.Parameters.AddWithValue("@Type", item.Type);
            cmd.Parameters.AddWithValue("@Data", item.Data);

            await using var rd = await cmd.ExecuteReaderAsync();
            return await rd.ReadAsync() ? Map(rd) : null;
        }

        // UPDATE
        public async Task<Biometric?> UpdateBiometric(int biometricId, Biometric item)
        {
            const string sql = @"
UPDATE Biometric
SET StaffId = @StaffId,
    Type    = @Type,
    Data    = @Data
OUTPUT INSERTED.*
WHERE BiometricId = @BiometricId;";

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@BiometricId", biometricId);
            cmd.Parameters.AddWithValue("@StaffId", item.StaffId);
            cmd.Parameters.AddWithValue("@Type", item.Type);
            cmd.Parameters.AddWithValue("@Data", item.Data);

            await using var rd = await cmd.ExecuteReaderAsync();
            return await rd.ReadAsync() ? Map(rd) : null;
        }

        // DELETE
        public async Task<Biometric?> DeleteBiometric(int biometricId)
        {
            const string sql = @"DELETE FROM Biometric OUTPUT DELETED.* WHERE BiometricId = @BiometricId;";

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@BiometricId", biometricId);

            await using var rd = await cmd.ExecuteReaderAsync();
            return await rd.ReadAsync() ? Map(rd) : null;
        }
    }
}
