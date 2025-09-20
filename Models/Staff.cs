using System.ComponentModel.DataAnnotations;

namespace RestfulAPI_FarmTimeManagement.Models
{
    public class Staff
    {
        public int StaffId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Email { get; set; }

        // =================================================================
        // Bug Fix: Phone Number Validation
        // Developer: Tim
        // Date: 2025-09-21
        // Description: Add phone number format validation
        // Issue: Invalid phone numbers like "1w2r3r" are accepted
        // =================================================================
        [RegularExpression(@"^[\+]?[0-9]{8,15}$",
            ErrorMessage = "Phone number must be 8-15 digits with optional + prefix")]
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
