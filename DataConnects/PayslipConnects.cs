using Microsoft.Data.SqlClient;
using RestfulAPI_FarmTimeManagement.Models;
using System.Data;

namespace RestfulAPI_FarmTimeManagement.DataConnects
{
    public class PayslipConnects
    {
        private SqlConnection Conn() => new SqlConnection(Config._connString);

        private static Payslip Map(SqlDataReader rd)
        {
            int iPayslipId = rd.GetOrdinal("PayslipId");
            int iStaffId = rd.GetOrdinal("StaffId");
            int iStandardPayRate = rd.GetOrdinal("StandardPayRate");
            int iWeekStartDate = rd.GetOrdinal("WeekStartDate");
            int iTotalHoursWorked = rd.GetOrdinal("TotalHoursWorked");
            int iGrossWeeklyPay = rd.GetOrdinal("GrossWeeklyPay");
            int iAnnualIncome = rd.GetOrdinal("AnnualIncome");
            int iAnnualTax = rd.GetOrdinal("AnnualTax");
            int iWeeklyPAYG = rd.GetOrdinal("WeeklyPAYG");
            int iNetPay = rd.GetOrdinal("NetPay");
            int iEmployerSuperannuation = rd.GetOrdinal("EmployerSuperannuation");
            int iDateCreated = rd.GetOrdinal("DateCreated");

            return new Payslip
            {
                PayslipId = rd.GetInt32(iPayslipId),
                StaffId = rd.GetInt32(iStaffId),
                StandardPayRate = rd.GetDecimal(iStandardPayRate),
                WeekStartDate = rd.GetDateTime(iWeekStartDate),
                TotalHoursWorked = rd.GetDecimal(iTotalHoursWorked),
                GrossWeeklyPay = rd.GetDecimal(iGrossWeeklyPay),
                AnnualIncome = rd.GetDecimal(iAnnualIncome),
                AnnualTax = rd.GetDecimal(iAnnualTax),
                WeeklyPAYG = rd.GetDecimal(iWeeklyPAYG),
                NetPay = rd.GetDecimal(iNetPay),
                EmployerSuperannuation = rd.GetDecimal(iEmployerSuperannuation),
                DateCreated = rd.GetDateTime(iDateCreated)
            };
        }

        // QUERY
        public async Task<List<Payslip>> QueryPayslip(string? querySql = null)
        {
            querySql ??= "SELECT PayslipId, StaffId, StandardPayRate, WeekStartDate, TotalHoursWorked, GrossWeeklyPay, AnnualIncome, AnnualTax, WeeklyPAYG, NetPay, EmployerSuperannuation, DateCreated FROM Payslip";
            var list = new List<Payslip>();

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(querySql, conn);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
                list.Add(Map(rd));
            return list;
        }

        // GET BY ID
        public async Task<Payslip?> GetPayslipById(int payslipId)
        {
            const string sql = "SELECT PayslipId, StaffId, StandardPayRate, WeekStartDate, TotalHoursWorked, GrossWeeklyPay, AnnualIncome, AnnualTax, WeeklyPAYG, NetPay, EmployerSuperannuation, DateCreated FROM Payslip WHERE PayslipId = @PayslipId";

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PayslipId", payslipId);

            await using var rd = await cmd.ExecuteReaderAsync();
            return await rd.ReadAsync() ? Map(rd) : null;
        }

        // GET BY STAFF ID
        public async Task<List<Payslip>> GetPayslipsByStaffId(int staffId)
        {
            const string sql = "SELECT PayslipId, StaffId, StandardPayRate, WeekStartDate, TotalHoursWorked, GrossWeeklyPay, AnnualIncome, AnnualTax, WeeklyPAYG, NetPay, EmployerSuperannuation, DateCreated FROM Payslip WHERE StaffId = @StaffId ORDER BY WeekStartDate DESC";

            var list = new List<Payslip>();
            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@StaffId", staffId);

            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
                list.Add(Map(rd));
            return list;
        }

        // GET BY STAFF ID AND WEEK
        public async Task<Payslip?> GetPayslipByStaffIdAndWeek(int staffId, DateTime weekStartDate)
        {
            const string sql = "SELECT PayslipId, StaffId, StandardPayRate, WeekStartDate, TotalHoursWorked, GrossWeeklyPay, AnnualIncome, AnnualTax, WeeklyPAYG, NetPay, EmployerSuperannuation, DateCreated FROM Payslip WHERE StaffId = @StaffId AND WeekStartDate = @WeekStartDate";

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@StaffId", staffId);
            cmd.Parameters.AddWithValue("@WeekStartDate", weekStartDate.Date);

            await using var rd = await cmd.ExecuteReaderAsync();
            return await rd.ReadAsync() ? Map(rd) : null;
        }

        // CREATE
        public async Task<Payslip?> CreatePayslip(Payslip item)
        {
            const string sql = @"
INSERT INTO Payslip (StaffId, StandardPayRate, WeekStartDate, TotalHoursWorked, GrossWeeklyPay, AnnualIncome, AnnualTax, WeeklyPAYG, NetPay, EmployerSuperannuation, DateCreated)
OUTPUT INSERTED.PayslipId, INSERTED.StaffId, INSERTED.StandardPayRate, INSERTED.WeekStartDate, INSERTED.TotalHoursWorked, INSERTED.GrossWeeklyPay, INSERTED.AnnualIncome, INSERTED.AnnualTax, INSERTED.WeeklyPAYG, INSERTED.NetPay, INSERTED.EmployerSuperannuation, INSERTED.DateCreated
VALUES (@StaffId, @StandardPayRate, @WeekStartDate, @TotalHoursWorked, @GrossWeeklyPay, @AnnualIncome, @AnnualTax, @WeeklyPAYG, @NetPay, @EmployerSuperannuation, @DateCreated);";

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@StaffId", item.StaffId);
            cmd.Parameters.AddWithValue("@StandardPayRate", item.StandardPayRate);
            cmd.Parameters.AddWithValue("@WeekStartDate", item.WeekStartDate.Date);
            cmd.Parameters.AddWithValue("@TotalHoursWorked", item.TotalHoursWorked);
            cmd.Parameters.AddWithValue("@GrossWeeklyPay", item.GrossWeeklyPay);
            cmd.Parameters.AddWithValue("@AnnualIncome", item.AnnualIncome);
            cmd.Parameters.AddWithValue("@AnnualTax", item.AnnualTax);
            cmd.Parameters.AddWithValue("@WeeklyPAYG", item.WeeklyPAYG);
            cmd.Parameters.AddWithValue("@NetPay", item.NetPay);
            cmd.Parameters.AddWithValue("@EmployerSuperannuation", item.EmployerSuperannuation);
            cmd.Parameters.AddWithValue("@DateCreated", item.DateCreated);

            await using var rd = await cmd.ExecuteReaderAsync();
            return await rd.ReadAsync() ? Map(rd) : null;
        }

        // UPDATE
        public async Task<Payslip?> UpdatePayslip(int payslipId, Payslip item)
        {
            const string sql = @"
UPDATE Payslip
SET StaffId = @StaffId,
    StandardPayRate = @StandardPayRate,
    WeekStartDate = @WeekStartDate,
    TotalHoursWorked = @TotalHoursWorked,
    GrossWeeklyPay = @GrossWeeklyPay,
    AnnualIncome = @AnnualIncome,
    AnnualTax = @AnnualTax,
    WeeklyPAYG = @WeeklyPAYG,
    NetPay = @NetPay,
    EmployerSuperannuation = @EmployerSuperannuation,
    DateCreated = @DateCreated
OUTPUT INSERTED.PayslipId, INSERTED.StaffId, INSERTED.StandardPayRate, INSERTED.WeekStartDate, INSERTED.TotalHoursWorked, INSERTED.GrossWeeklyPay, INSERTED.AnnualIncome, INSERTED.AnnualTax, INSERTED.WeeklyPAYG, INSERTED.NetPay, INSERTED.EmployerSuperannuation, INSERTED.DateCreated
WHERE PayslipId = @PayslipId;";

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PayslipId", payslipId);
            cmd.Parameters.AddWithValue("@StaffId", item.StaffId);
            cmd.Parameters.AddWithValue("@StandardPayRate", item.StandardPayRate);
            cmd.Parameters.AddWithValue("@WeekStartDate", item.WeekStartDate.Date);
            cmd.Parameters.AddWithValue("@TotalHoursWorked", item.TotalHoursWorked);
            cmd.Parameters.AddWithValue("@GrossWeeklyPay", item.GrossWeeklyPay);
            cmd.Parameters.AddWithValue("@AnnualIncome", item.AnnualIncome);
            cmd.Parameters.AddWithValue("@AnnualTax", item.AnnualTax);
            cmd.Parameters.AddWithValue("@WeeklyPAYG", item.WeeklyPAYG);
            cmd.Parameters.AddWithValue("@NetPay", item.NetPay);
            cmd.Parameters.AddWithValue("@EmployerSuperannuation", item.EmployerSuperannuation);
            cmd.Parameters.AddWithValue("@DateCreated", item.DateCreated);

            await using var rd = await cmd.ExecuteReaderAsync();
            return await rd.ReadAsync() ? Map(rd) : null;
        }

        // DELETE
        public async Task<Payslip?> DeletePayslip(int payslipId)
        {
            const string sql = @"
DELETE FROM Payslip
OUTPUT DELETED.PayslipId, DELETED.StaffId, DELETED.StandardPayRate, DELETED.WeekStartDate, DELETED.TotalHoursWorked, DELETED.GrossWeeklyPay, DELETED.AnnualIncome, DELETED.AnnualTax, DELETED.WeeklyPAYG, DELETED.NetPay, DELETED.EmployerSuperannuation, DELETED.DateCreated
WHERE PayslipId = @PayslipId;";

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PayslipId", payslipId);

            await using var rd = await cmd.ExecuteReaderAsync();
            return await rd.ReadAsync() ? Map(rd) : null;
        }

        // CHECK IF PAYSLIP EXISTS FOR STAFF AND WEEK
        public async Task<bool> PayslipExists(int staffId, DateTime weekStartDate)
        {
            const string sql = "SELECT COUNT(1) FROM Payslip WHERE StaffId = @StaffId AND WeekStartDate = @WeekStartDate";

            await using var conn = Conn();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@StaffId", staffId);
            cmd.Parameters.AddWithValue("@WeekStartDate", weekStartDate.Date);

            var count = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(count) > 0;
        }
    }
}
