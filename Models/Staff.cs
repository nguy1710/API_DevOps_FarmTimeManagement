namespace RestfulAPI_FarmTimeManagement.Models
{
    public class Staff
    {
        public int StaffId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Password { get; set; } // Should be hash in next sprint
        public string? Address { get; set; }
        public string? ContractType { get; set; }
        public string? Role { get; set; } // Admin , Staff, Engineer...
        public decimal? StandardHoursPerWeek { get; set; }
        public decimal? StandardPayRate { get; set; }
        public decimal? OvertimePayRate { get; set; } 
    }
}
