namespace RestfulAPI_FarmTimeManagement.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public DateTime Timestamp { get; set; }     // DATETIME2(0)
        public int StaffId { get; set; }            // FK -> Staff(StaffId)
        public int? DeviceId { get; set; }          // FK -> Device(DeviceId), có thể null
        public string EventType { get; set; } = null!; // giữ nguyên tên cột theo DB
        public string? Reason { get; set; }
    }
}
