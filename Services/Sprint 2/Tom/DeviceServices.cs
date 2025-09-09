using Microsoft.AspNetCore.Http.HttpResults;
using RestfulAPI_FarmTimeManagement.DataConnects;
using RestfulAPI_FarmTimeManagement.Models;
using RestfulAPI_FarmTimeManagement.Services.Sprint1.Tom;

namespace RestfulAPI_FarmTimeManagement.Services.Sprint_2.Tom
{
    public class DeviceServices
    {


        public static async Task<List<Device>> GetAllDevices()
        {
            var deviceConnects = new DeviceConnects();
            var querystring = @"SELECT * FROM Device ORDER BY DeviceId DESC";
            var result = await deviceConnects.QueryDevice(querystring);
            return result;
        }

        public static async Task<List<Device>> QueryDevices(string querystring)
        {
            var deviceConnects = new DeviceConnects();
            var result = await deviceConnects.QueryDevice(querystring);
            return result;
        }

        public static async Task<Device?> CreateDevice(Device device)
        {
            var deviceConnects = new DeviceConnects();
            var created = await deviceConnects.CreateDevice(device);


            History history = new History
            {
                Action = "Create device",
                Details = $"device {created.Type} {created.Location} was created.",
               // Actor = result.Email,
                Result = "Succeed",
            };
            History result_hiscreated = await HistoryServices.CreateHistory(history);

            if (result_hiscreated!=null)
            {
                return created;

            }

            throw new Exception("Create device failed");

        }


        public static async Task<Device?> UpdateDevice(int id, Device device)
        {
            var deviceConnects = new DeviceConnects();
            var updated = await deviceConnects.UpdateDevice(id, device);



            History history = new History
            {
                Action = "Update device",
                Details = $"device {updated.Type} {updated.Location} was updated.",
                // Actor = result.Email,
                Result = "Succeed",
            };
            History result_hisupdated = await HistoryServices.CreateHistory(history);

            if (result_hisupdated != null)
            {
                return updated;

            }

            throw new Exception("Updated device failed"); 
             
        }


        public static async Task<Device?> DeleteDevice(int id)
        {
            var deviceConnects = new DeviceConnects();
            var deleted = await deviceConnects.DeleteDevice(id);

             

            History history = new History
            {
                Action = "Delete device",
                Details = $"device {deleted.Type} {deleted.Location} was deleted.",
                // Actor = result.Email,
                Result = "Succeed",
            };
            History result_hisdeleted = await HistoryServices.CreateHistory(history);

            if (result_hisdeleted != null)
            {
                return deleted; 
            }

            throw new Exception("Delete device failed"); 
             
        }

    }
}
