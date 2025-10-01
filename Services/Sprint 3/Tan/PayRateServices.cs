using RestfulAPI_FarmTimeManagement.DataConnects;
using RestfulAPI_FarmTimeManagement.Models;

namespace RestfulAPI_FarmTimeManagement.Services.Sprint3.Tan
{
    public static class PayRateServices
    {
        private static async Task<List<Staff>> GetStaffByRoleAndContract(string role, string contractType)
        {
            var staffConnects = new StaffConnects();
            var query = $@"SELECT * FROM [Staff]
                          WHERE [Role] = '{role}' AND [ContractType] = '{contractType}'
                          ORDER BY [StaffId]";
            return await staffConnects.QueryStaff(query);
        }

        public static async Task<Staff?> GetStaffById(int staffId)
        {
            var staffConnects = new StaffConnects();
            var query = $"SELECT * FROM [Staff] WHERE [StaffId] = {staffId}";
            var results = await staffConnects.QueryStaff(query);
            return results.FirstOrDefault();
        }

        public static async Task<Staff?> UpdateStaffPayRates(int staffId, decimal standardPayRate, decimal overtimePayRate)
        {
            var staffConnects = new StaffConnects();

            // First get the existing staff
            var existingStaff = await GetStaffById(staffId);
            if (existingStaff == null) return null;

            // Update the pay rates
            existingStaff.StandardPayRate = standardPayRate;
            existingStaff.OvertimePayRate = overtimePayRate;

            return await staffConnects.UpdateStaff(staffId, existingStaff);
        }

        public static async Task<List<Staff>> BulkUpdatePayRatesByRoleContract(string role, string contractType,
            decimal standardPayRate, decimal overtimePayRate)
        {
            var staffConnects = new StaffConnects();
            var staffList = await GetStaffByRoleAndContract(role, contractType);
            var updatedStaff = new List<Staff>();

            foreach (var staff in staffList)
            {
                staff.StandardPayRate = standardPayRate;
                staff.OvertimePayRate = overtimePayRate;
                var updated = await staffConnects.UpdateStaff(staff.StaffId, staff);
                updatedStaff.Add(updated);
            }

            return updatedStaff;
        }

        public static async Task<List<Staff>> InitializeDefaultPayRates()
        {
            var defaultRates = GetDefaultPayRates();
            var updatedStaff = new List<Staff>();

            foreach (var roleRates in defaultRates)
            {
                string role = roleRates.Key;
                foreach (var contractRates in roleRates.Value)
                {
                    string contractType = contractRates.Key;
                    var rateInfo = contractRates.Value;

                    var updated = await BulkUpdatePayRatesByRoleContract(
                        role, contractType, rateInfo.StandardRate, rateInfo.OvertimeRate);
                    updatedStaff.AddRange(updated);
                }
            }

            return updatedStaff;
        }

        // Get default pay rates based on Horticulture Award 2025
        public static Dictionary<string, Dictionary<string, PayRateInfo>> GetDefaultPayRates()
        {
            return new Dictionary<string, Dictionary<string, PayRateInfo>>
            {
                ["Worker"] = new Dictionary<string, PayRateInfo>
                {
                    ["Full-time"] = new PayRateInfo { StandardRate = 25.00m, OvertimeRate = 37.50m },
                    ["Part-time"] = new PayRateInfo { StandardRate = 25.00m, OvertimeRate = 37.50m },
                    ["Casual"] = new PayRateInfo { StandardRate = 31.25m, OvertimeRate = 46.87m }
                },
                ["Manager"] = new Dictionary<string, PayRateInfo>
                {
                    ["Full-time"] = new PayRateInfo { StandardRate = 35.00m, OvertimeRate = 52.50m },
                    ["Part-time"] = new PayRateInfo { StandardRate = 35.00m, OvertimeRate = 52.50m },
                    ["Casual"] = new PayRateInfo { StandardRate = 43.75m, OvertimeRate = 65.62m }
                }
            };
        }

        public static PayRateInfo? GetDefaultPayRateForRoleContract(string role, string contractType)
        {
            var defaults = GetDefaultPayRates();
            return defaults.ContainsKey(role) && defaults[role].ContainsKey(contractType)
                ? defaults[role][contractType]
                : null;
        }
    }

    public class PayRateInfo
    {
        public decimal StandardRate { get; set; }
        public decimal OvertimeRate { get; set; }
        public decimal WeekendRate => StandardRate * 2; // 2x multiplier for weekends
    }
}