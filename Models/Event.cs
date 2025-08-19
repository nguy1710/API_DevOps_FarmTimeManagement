namespace RestfulAPI_FarmTimeManagement.Models
{
    public class Event
    {

        public long EventId { get; set; }          // BIGINT IDENTITY
        public int StaffId { get; set; }           // FK -> Staff
        public DateTime TimeStamp { get; set; }    // NOT NULL
        public string? EventType { get; set; }     // e.g., CLOCK_IN / CLOCK_OUT / OVERRIDE...
        public string? Reason { get; set; }        // optional
        public int? DeviceId { get; set; }         // FK -> Device
        public int? AdminId { get; set; }          // FK -> Staff (admin)
    }
}
