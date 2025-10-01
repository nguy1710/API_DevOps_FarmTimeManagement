namespace RestfulAPI_FarmTimeManagement.Models
{
    public class Biometric
    {
        public int BiometricId { get; set; }
        public int StaffId { get; set; }            // FK -> Staff(StaffId)
        public string Type { get; set; } = null!;   // 'finger print','face','Card',...
        public string Data { get; set; } = null!;   // có thể là template/chuỗi đã mã hoá
    }
}
