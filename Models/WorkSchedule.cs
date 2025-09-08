namespace RestfulAPI_FarmTimeManagement.Models
{
    public class WorkSchedule
    {
        public int ScheduleId { get; set; }
        public int StaffId { get; set; }                 // FK -> Staff(StaffId)
        public DateTime StartTime { get; set; }          // DATETIME2(0)
        public DateTime EndTime { get; set; }            // DATETIME2(0)
        public int ScheduleHours { get; set; }
    }
}
