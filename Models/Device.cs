namespace RestfulAPI_FarmTimeManagement.Models
{
    public class Device
    {
        public int DeviceId { get; set; }
        public string Location { get; set; } = null!; // "lat,long"
        public string Type { get; set; } = null!;
        public string Status { get; set; } = null!;
    }
}
