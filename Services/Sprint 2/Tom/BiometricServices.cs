using RestfulAPI_FarmTimeManagement.DataConnects;
using RestfulAPI_FarmTimeManagement.Models;
using RestfulAPI_FarmTimeManagement.Services.Sprint1.Tom; // HistoryServices

namespace RestfulAPI_FarmTimeManagement.Services.Sprint_2.Tom
{
    public static class BiometricServices
    {
        public static async Task<List<Biometric>> GetAllBiometrics()
        {
            var connects = new BiometricConnects();
            var query = @"SELECT * FROM Biometric ORDER BY BiometricId DESC";
            return await connects.QueryBiometric(query);
        }

        public static async Task<List<Biometric>> QueryBiometrics(string querystring)
        {
            var connects = new BiometricConnects();
            return await connects.QueryBiometric(querystring);
        }




        public static async Task<Staff?> GetStaff_From_Bio_Card(string bio_data)
        {
            string query = $@"SELECT s.* FROM Staff s
                              JOIN Biometric b ON s.StaffId = b.StaffId
                              WHERE b.Type = 'Card' AND b.Data = '{bio_data}'";

           List<Staff> staffs = await StaffsServices.QuerryStaffs(query);

            if (staffs.Count == 1)
            {
                return staffs[0];
            }
            return null;
        }








        public static async Task<Biometric?> CreateBiometric(Biometric item)
        {
            var connects = new BiometricConnects();
            var created = await connects.CreateBiometric(item);

            // Log history giống DeviceServices
            History history = new History
            {
                Action = "Create biometric",
                Details = $"Biometric {created?.Type} for StaffId={created?.StaffId} was created.",
                Result = "Succeed"
            };
            var his = await HistoryServices.CreateHistory(history);
            if (his != null) return created;

            throw new Exception("Create biometric failed");
        }

        public static async Task<Biometric?> UpdateBiometric(int id, Biometric item)
        {
            var connects = new BiometricConnects();
            var updated = await connects.UpdateBiometric(id, item);

            History history = new History
            {
                Action = "Update biometric",
                Details = $"Biometric {updated?.Type} for StaffId={updated?.StaffId} was updated.",
                Result = "Succeed"
            };
            var his = await HistoryServices.CreateHistory(history);
            if (his != null) return updated;

            throw new Exception("Update biometric failed");
        }

        public static async Task<Biometric?> DeleteBiometric(int id)
        {
            var connects = new BiometricConnects();
            var deleted = await connects.DeleteBiometric(id);

            History history = new History
            {
                Action = "Delete biometric",
                Details = $"Biometric {deleted?.Type} for StaffId={deleted?.StaffId} was deleted.",
                Result = "Succeed"
            };
            var his = await HistoryServices.CreateHistory(history);
            if (his != null) return deleted;

            throw new Exception("Delete biometric failed");
        }
    }
}
