using RestfulAPI_FarmTimeManagement.DataConnects;
using RestfulAPI_FarmTimeManagement.Models;
using RestfulAPI_FarmTimeManagement.Services.Sprint_2.Tan;
using RestfulAPI_FarmTimeManagement.Services.Sprint_2.Tom;
using RestfulAPI_FarmTimeManagement.Services.Sprint1.Tom;

namespace RestfulAPI_FarmTimeManagement.Services.Sprint_4.Tom
{
    public static class PayslipServices
    {
        /// <summary>
        /// Calculates total hours worked by a staff member for a specific week
        /// </summary>
        /// <param name="staffId">Staff member ID</param>
        /// <param name="weekStartDate">Any date within the week to calculate</param>
        /// <returns>Total hours worked (decimal)</returns>
        public static async Task<decimal> CalculateTotalHourWorker(int staffId, DateTime weekStartDate)
        {
            // Calculate the start and end of the week (Monday to Sunday)
            var weekStart = GetWeekStart(weekStartDate);
            var weekEnd = weekStart.AddDays(7);

            // Step 1: Get all WorkSchedules for the week
            var workSchedules = await RosterServices.GetSchedulesByStaffId(staffId, weekStart);
            
            // Step 2: Get all Events for the week
            var events = await EventServices.GetEventsByStaffId(staffId, weekStart);

            // Step 3: Initialize total hours
            decimal totalHours = 0;

            // Step 4: Add scheduled hours for each WorkSchedule
            foreach (var schedule in workSchedules)
            {
                totalHours += schedule.ScheduleHours;

                // Step 5: Check for Break events within this schedule's time range
                var breakEvents = events.Where(e => 
                    e.EventType == "Break" &&
                    e.Timestamp >= schedule.StartTime &&
                    e.Timestamp <= schedule.EndTime
                ).ToList();

                // Step 6: Deduct 0.5 hours for each Break event
                totalHours -= breakEvents.Count * 0.5m;
            }

            return Math.Max(0, totalHours); // Ensure non-negative result
        }

        /// <summary>
        /// Creates a new payslip for a staff member for a specific week or returns existing payslip if already exists
        /// </summary>
        /// <param name="staffId">Staff member ID</param>
        /// <param name="weekStartDate">Any date within the week to calculate</param>
        /// <returns>Created Payslip object or existing payslip if already exists, null if failed</returns>
        public static async Task<Payslip?> CreatePayslip(int staffId, DateTime weekStartDate)
        {
            try
            {
                // Step 1: Get staff information including StandardPayRate
                var staff = await StaffsServices.GetStaffById(staffId);
                if (staff == null)
                {
                    return null; // Staff not found
                }

                // Step 2: Calculate total hours worked using existing method
                var totalHoursWorked = await CalculateTotalHourWorker(staffId, weekStartDate);

                // Step 3: Get the Monday of the week
                var weekStart = GetWeekStart(weekStartDate);

                // Step 4: Calculate payroll using our own formulas
                var grossWeeklyPay = CalculateGrossWeeklyPay(weekStart, staff, totalHoursWorked);

                // Step 5: Check if payslip already exists for this staff and week
                var payslipConnects = new PayslipConnects();
                var existingPayslip = await payslipConnects.GetPayslipByStaffIdAndWeek(staffId, weekStart);
                if (existingPayslip != null)
                {
                    return existingPayslip; // Return existing payslip for this week
                }

                // Step 6: Calculate other payroll values
                var annualIncome = CalculateAnnualIncome(grossWeeklyPay);
                var annualTax = CalculateAnnualTax(annualIncome);
                var weeklyPAYG = annualTax / 52;
                var netPay = grossWeeklyPay - weeklyPAYG;
                var employerSuperannuation = CalculateEmployerSuperannuation(grossWeeklyPay);

                // Step 7: Create new Payslip object
                var newPayslip = new Payslip
                {
                    StaffId = staffId,
                    StandardPayRate = staff.StandardPayRate ?? 0m,
                    WeekStartDate = weekStart.Date,
                    TotalHoursWorked = totalHoursWorked,
                    GrossWeeklyPay = grossWeeklyPay,
                    AnnualIncome = annualIncome,
                    AnnualTax = annualTax,
                    WeeklyPAYG = weeklyPAYG,
                    NetPay = netPay,
                    EmployerSuperannuation = employerSuperannuation,
                    DateCreated = DateTime.Now
                };

                // Step 8: Save to database
                var createdPayslip = await payslipConnects.CreatePayslip(newPayslip);
                return createdPayslip;
            }
            catch (Exception)
            {
                return null; // Return null on any error
            }
        }

        /// <summary>
        /// Calculates gross weekly pay based on total hours worked
        /// Simplified calculation: TotalHoursWorked * StandardPayRate
        /// </summary>
        private static decimal CalculateGrossWeeklyPay(DateTime weekStart, Staff staff, decimal totalHoursWorked)
        {
            // Simple calculation: hours worked * pay rate
            // For more complex overtime calculations, we would need to implement the full PayRollServices logic
            decimal payRate = staff.StandardPayRate ?? 0m;
            return totalHoursWorked * payRate;
        }

        /// <summary>
        /// Calculate Annual Income = Gross Weekly Pay × 52
        /// </summary>
        private static decimal CalculateAnnualIncome(decimal grossWeeklyPay)
        {
            return grossWeeklyPay * 52;
        }

        /// <summary>
        /// Calculate Annual Tax using ATO resident tax thresholds
        /// </summary>
        private static decimal CalculateAnnualTax(decimal annualIncome)
        {
            decimal tax = 0;

            if (annualIncome <= 18200)
            {
                // $0 – $18,200 → Tax = $0
                tax = 0;
            }
            else if (annualIncome <= 45000)
            {
                // $18,201 – $45,000 → Tax = 16% × (Income – 18,200)
                tax = 0.16m * (annualIncome - 18200);
            }
            else if (annualIncome <= 135000)
            {
                // $45,001 – $135,000 → Tax = $4,288 + 30% × (Income – 45,000)
                tax = 4288 + 0.30m * (annualIncome - 45000);
            }
            else if (annualIncome <= 190000)
            {
                // $135,001 – $190,000 → Tax = $31,288 + 37% × (Income – 135,000)
                tax = 31288 + 0.37m * (annualIncome - 135000);
            }
            else
            {
                // > $190,000 → Tax = $51,738 + 45% × (Income – 190,000)
                tax = 51738 + 0.45m * (annualIncome - 190000);
            }

            return tax;
        }

        /// <summary>
        /// Calculate Employer Superannuation = Gross Pay × Superannuation Guarantee (SG) Rate
        /// Current SG = 12% from 1 July 2025
        /// </summary>
        private static decimal CalculateEmployerSuperannuation(decimal grossPay, decimal sgRate = 0.12m)
        {
            return grossPay * sgRate;
        }


        /// <summary>
        /// Deletes a payslip by ID
        /// </summary>
        /// <param name="payslipId">Payslip ID to delete</param>
        /// <returns>Deleted Payslip object or null if not found</returns>
        public static async Task<Payslip?> DeletePayslip(int payslipId)
        {
            try
            {
                var payslipConnects = new PayslipConnects();
                var deletedPayslip = await payslipConnects.DeletePayslip(payslipId);
                return deletedPayslip;
            }
            catch (Exception)
            {
                return null; // Return null on any error
            }
        }

        /// <summary>
        /// Queries all payslips
        /// </summary>
        /// <returns>List of all payslips</returns>
        public static async Task<List<Payslip>> QueryPayslips()
        {
            try
            {
                var payslipConnects = new PayslipConnects();
                var payslips = await payslipConnects.QueryPayslip();
                return payslips;
            }
            catch (Exception)
            {
                return new List<Payslip>(); // Return empty list on error
            }
        }

        /// <summary>
        /// Gets all payslips for a specific staff member
        /// </summary>
        /// <param name="staffId">Staff member ID</param>
        /// <returns>List of payslips for the staff member</returns>
        public static async Task<List<Payslip>> GetPayslipsByStaffId(int staffId)
        {
            try
            {
                var payslipConnects = new PayslipConnects();
                var payslips = await payslipConnects.GetPayslipsByStaffId(staffId);
                return payslips;
            }
            catch (Exception)
            {
                return new List<Payslip>(); // Return empty list on error
            }
        }

        /// <summary>
        /// Creates a payslip for a staff member with custom date range and pay rate
        /// </summary>
        /// <param name="staffId">Staff member ID</param>
        /// <param name="standardPayRate">Custom pay rate for this payslip</param>
        /// <param name="dateStart">Start date of the period</param>
        /// <param name="dateEnd">End date of the period</param>
        /// <returns>Created Payslip object or null if failed</returns>
        public static async Task<Payslip?> CreatePayslip_Special(int staffId, decimal standardPayRate, DateTime dateStart, DateTime dateEnd)
        {
            try
            {
                // Step 1: Get staff information
                var staff = await StaffsServices.GetStaffById(staffId);
                if (staff == null)
                {
                    return null; // Staff not found
                }

                // Step 2: Calculate total hours worked for the custom date range
                var totalHoursWorked = await CalculateTotalHourWorker_CustomRange(staffId, dateStart, dateEnd);

                // Step 3: Calculate gross pay using custom pay rate
                var grossPay = totalHoursWorked * standardPayRate;

                // Step 4: Calculate weekly equivalent for annual calculations
                // If the period is not exactly 7 days, we need to normalize it
                var daysInPeriod = (dateEnd - dateStart).Days + 1;
                var weeklyEquivalent = grossPay * (7.0m / daysInPeriod);

                // Step 5: Calculate other payroll values based on weekly equivalent
                var annualIncome = weeklyEquivalent * 52;
                var annualTax = CalculateAnnualTax(annualIncome);
                var weeklyPAYG = annualTax / 52;
                var netPay = grossPay - (weeklyPAYG * (daysInPeriod / 7.0m));
                var employerSuperannuation = CalculateEmployerSuperannuation(grossPay);

                // Step 6: Create new Payslip object
                var newPayslip = new Payslip
                {
                    StaffId = staffId,
                    StandardPayRate = standardPayRate,
                    WeekStartDate = dateStart.Date,
                    TotalHoursWorked = totalHoursWorked,
                    GrossWeeklyPay = grossPay, // This is actually gross pay for the period
                    AnnualIncome = annualIncome,
                    AnnualTax = annualTax,
                    WeeklyPAYG = weeklyPAYG,
                    NetPay = netPay,
                    EmployerSuperannuation = employerSuperannuation,
                    DateCreated = DateTime.Now
                };

                // Step 7: Save to database
                var payslipConnects = new PayslipConnects();
                var createdPayslip = await payslipConnects.CreatePayslip(newPayslip);
                return createdPayslip;
            }
            catch (Exception)
            {
                return null; // Return null on any error
            }
        }

        /// <summary>
        /// Calculates total hours worked for a custom date range
        /// </summary>
        /// <param name="staffId">Staff member ID</param>
        /// <param name="dateStart">Start date of the period</param>
        /// <param name="dateEnd">End date of the period</param>
        /// <returns>Total hours worked (decimal)</returns>
        private static async Task<decimal> CalculateTotalHourWorker_CustomRange(int staffId, DateTime dateStart, DateTime dateEnd)
        {
            // Step 1: Get all WorkSchedules within the date range
            var workScheduleConnects = new WorkScheduleConnects();
            var startDate = dateStart.Date;
            var endDate = dateEnd.Date.AddDays(1); // Add 1 day to include the end date
            
            var query = $@"
                SELECT * FROM WorkSchedule 
                WHERE StaffId = {staffId}
                AND StartTime >= '{startDate:yyyy-MM-dd 00:00:00}'
                AND StartTime < '{endDate:yyyy-MM-dd 00:00:00}'
                ORDER BY StartTime";
            
            var workSchedules = await workScheduleConnects.QueryWorkSchedule(query);

            // Step 2: Get all Events within the date range
            var eventConnects = new EventConnects();
            var eventQuery = $@"
                SELECT * FROM Event 
                WHERE StaffId = {staffId}
                AND Timestamp >= '{startDate:yyyy-MM-dd 00:00:00}'
                AND Timestamp < '{endDate:yyyy-MM-dd 00:00:00}'
                ORDER BY Timestamp";
            
            var events = await eventConnects.QueryEvent(eventQuery);

            // Step 3: Initialize total hours
            decimal totalHours = 0;

            // Step 4: Add scheduled hours for each WorkSchedule
            foreach (var schedule in workSchedules)
            {
                totalHours += schedule.ScheduleHours;

                // Step 5: Check for Break events within this schedule's time range
                var breakEvents = events.Where(e => 
                    e.EventType == "Break" &&
                    e.Timestamp >= schedule.StartTime &&
                    e.Timestamp <= schedule.EndTime
                ).ToList();

                // Step 6: Deduct 0.5 hours for each Break event
                totalHours -= breakEvents.Count * 0.5m;
            }

            return Math.Max(0, totalHours); // Ensure non-negative result
        }

        /// <summary>
        /// Gets the start of the week (Monday) for a given date
        /// </summary>
        private static DateTime GetWeekStart(DateTime date)
        {
            var daysSinceMonday = ((int)date.DayOfWeek - 1 + 7) % 7;
            return date.Date.AddDays(-daysSinceMonday);
        }

    }

}
