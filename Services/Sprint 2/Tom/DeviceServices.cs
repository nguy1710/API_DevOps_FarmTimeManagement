using Microsoft.AspNetCore.Http;
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

        public static async Task<Device?> CreateDevice(Device device, HttpContext httpContext)
        {
            var deviceConnects = new DeviceConnects();
            var created = await deviceConnects.CreateDevice(device);


            if (created != null)
            {
                var his = await HistoryServices.CreateHistory(
                    "Create device",
                    "Succeed",
                    $"device {created.Type} {created.Location} was created.",
                    httpContext
                );
                if (his != null) return created;
            }

            return new Device {  DeviceId = -1,Status = "Failed due to system issue.",};
        }


        public static async Task<Device> UpdateDevice(int id, Device device, HttpContext httpContext)
        {
            var deviceConnects = new DeviceConnects();
            var updated = await deviceConnects.UpdateDevice(id, device);


            if (updated != null)
            {
                var his = await HistoryServices.CreateHistory(
                    "Update device",
                    "Succeed",
                    $"device {updated.Type} {updated.Location} was updated.",
                    httpContext
                );
                if (his != null) return updated;
            }


            return new Device
            {
                DeviceId = -1,
                Status = "Failed due to system issue."  
            };
        }


        public static async Task<Device> DeleteDevice(int id, HttpContext httpContext)
        {
            var deviceConnects = new DeviceConnects();
            var deleted = await deviceConnects.DeleteDevice(id);


            if (deleted != null)
            {
                var his = await HistoryServices.CreateHistory(
                    "Delete device",
                    "Succeed",
                    $"device {deleted.Type} {deleted.Location} was deleted.",
                    httpContext
                );
                if (his != null) return deleted;
            }

            return new Device
            {
                DeviceId = -1,
                Status = "Failed due to system issue.",
                Type = string.Empty,
                Location = string.Empty
            };
        }

    }
}
