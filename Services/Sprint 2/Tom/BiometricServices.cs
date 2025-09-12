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








        public static async Task<Biometric> CreateBiometric(Biometric item, HttpContext httpContext)
        {
            var connects = new BiometricConnects();

            var staff =   await  BiometricServices.GetStaff_From_Bio_Card(item.Data);
            if (staff != null)
            {
                return new Biometric
                {
                    BiometricId = -1,
                    StaffId = item?.StaffId ?? 0,
                    Type = item?.Type ?? string.Empty,
                    Data = "Failed due to duplicate Card Data."
                };
            }

            var created = await connects.CreateBiometric(item);

            if (created != null)
            {
                var his = await HistoryServices.CreateHistory(
                    "Create biometric",
                    "Succeed",
                    $"Biometric {created.Type} for StaffId={created.StaffId} was created.",
                    httpContext
                );
                if (his != null) return created;
            }

            return new Biometric
            {
                BiometricId = -1,
                StaffId = item?.StaffId ?? 0,
                Type = item?.Type ?? string.Empty,
                Data = "Failed due to system issue."
            };
        }

        public static async Task<Biometric> UpdateBiometric(int id, Biometric item, HttpContext httpContext)
        {
            var connects = new BiometricConnects();
            var updated = await connects.UpdateBiometric(id, item);
            if (updated != null)
            {
                var his = await HistoryServices.CreateHistory(
                    "Update biometric",
                    "Succeed",
                    $"Biometric {updated.Type} for StaffId={updated.StaffId} was updated.",
                    httpContext
                );
                if (his != null) return updated;
            }

            return new Biometric
            {
                BiometricId = -1,
                StaffId = item?.StaffId ?? 0,
                Type = item?.Type ?? string.Empty,
                Data = "Failed due to system issue."
            };
        }

        public static async Task<Biometric> DeleteBiometric(int id, HttpContext httpContext)
        {
            var connects = new BiometricConnects();
            var deleted = await connects.DeleteBiometric(id);

            if (deleted != null)
            {
                var his = await HistoryServices.CreateHistory(
                    "Delete biometric",
                    "Succeed",
                    $"Biometric {deleted.Type} for StaffId={deleted.StaffId} was deleted.",
                    httpContext
                );
                if (his != null) return deleted;
            }

            return new Biometric
            {
                BiometricId = -1,
                StaffId = 0,
                Type = string.Empty,
                Data = "Failed due to system issue."
            };
        }
    }
}
