using System;

namespace RestfulAPI_FarmTimeManagement.Models
{
    public class Payslip
    {
        public int PayslipId { get; set; }
        public int StaffId { get; set; }
        public decimal StandardPayRate { get; set; }
        public DateTime WeekStartDate { get; set; }
        public decimal TotalHoursWorked { get; set; }
        public decimal GrossWeeklyPay { get; set; }
        public decimal AnnualIncome { get; set; }
        public decimal AnnualTax { get; set; }
        public decimal WeeklyPAYG { get; set; }
        public decimal NetPay { get; set; }
        public decimal EmployerSuperannuation { get; set; }
        public DateTime DateCreated { get; set; }
    }
}


