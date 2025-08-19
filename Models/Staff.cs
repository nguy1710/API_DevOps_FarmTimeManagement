namespace RestfulAPI_FarmTimeManagement.Models
{
    public class Staff
    {
        public int StaffId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? ContractType { get; set; }
        public bool? IsActive { get; set; }
        public string? Role { get; set; }
        public decimal? StandardHoursPerWeek { get; set; }
        public decimal? StandardPayRate { get; set; }
        public decimal? OvertimePayRate { get; set; }


    }
}
