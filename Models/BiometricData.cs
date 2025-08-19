namespace RestfulAPI_FarmTimeManagement.Models
{
    public class BiometricData
    {
        public int BiometricId { get; set; }
        public int StaffId { get; set; }
        public string Template { get; set; } = null!;
    }
}
