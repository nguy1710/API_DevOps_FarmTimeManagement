using RestfulAPI_FarmTimeManagement.Models;
using RestfulAPI_FarmTimeManagement.Services.Sprint_2.Tom;
using RestfulAPI_FarmTimeManagement.Services.Sprint3.Tan;

namespace RestfulAPI_FarmTimeManagement.Services.Sprint_3.Tom
{
    public static class PayRollServices
    { 

        public static async Task<PayrollSummary> CalculateCompletePayroll(DateTime date, Staff staff,bool is_SpecicalPayrate = false )
        {
            // Get the actual Monday of the week for the provided date
            DateTime actualMonday = GetMondayOfWeek(date);

            // Apply special pay rate if requested
            if (is_SpecicalPayrate)
            {
                var defaultPayRate = PayRateServices.GetDefaultPayRateForRoleContract(staff.Role!, staff.ContractType!);
                if (defaultPayRate != null)
                {
                    staff.StandardPayRate = defaultPayRate.StandardRate;
                    // Note: OvertimePayRate is calculated as 1.5x StandardPayRate in CalculatePayRollWeek
                }
            }

            // Calculate total hours worked using private helper method
            List<HoursWperDayofWeek> weekHours = await CalculateWeeklyHours(actualMonday, staff.StaffId);
            decimal totalHours = weekHours.Sum(h => h.HoursWorked);

            // Step 1: Calculate Gross Weekly Pay
            decimal grossWeeklyPay = await CalculatePayRollWeek(actualMonday, staff);

            // Step 2: Calculate Annual Income
            decimal annualIncome = CalculateAnnualIncome(grossWeeklyPay);

            // Step 3: Calculate PAYG
            decimal annualTax = CalculateAnnualTax(annualIncome);
            decimal weeklyPAYG = annualTax / 52;

            // Step 4: Calculate Net Pay
            decimal netPay = grossWeeklyPay - weeklyPAYG;

            // Step 5: Calculate Employer Superannuation
            decimal superannuation = CalculateEmployerSuperannuation(grossWeeklyPay);

            return new PayrollSummary
            {
                StaffId = staff.StaffId,
                StaffName = $"{staff.FirstName} {staff.LastName}",
                WeekStartDate = actualMonday,
                TotalHoursWorked = totalHours,
                GrossWeeklyPay = grossWeeklyPay,
                AnnualIncome = annualIncome,
                AnnualTax = annualTax,
                WeeklyPAYG = weeklyPAYG,
                NetPay = netPay,
                EmployerSuperannuation = superannuation
            };
        }


        // Private helper method to get the Monday of the week for any given date
        private static DateTime GetMondayOfWeek(DateTime date)
        {
            // Calculate how many days since Monday
            // DayOfWeek: Sunday=0, Monday=1, Tuesday=2, ..., Saturday=6
            int daysSinceMonday = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            
            // Return the Monday of that week
            return date.Date.AddDays(-daysSinceMonday);
        }


 





        // Private helper method to calculate hours worked for each day of the week
        private static async Task<List<HoursWperDayofWeek>> CalculateWeeklyHours(DateTime Monday_date, int staffId)
        {
            DateTime startDate = Monday_date.Date;
            DateTime endDate = startDate.AddDays(7);

            string query = $@"
                SELECT * FROM [Event] 
                WHERE StaffId = {staffId} 
                  AND [Timestamp] >= '{startDate:yyyy-MM-dd HH:mm:ss}' 
                  AND [Timestamp] < '{endDate:yyyy-MM-dd HH:mm:ss}'
                  AND EventType IN ('Clock in', 'Clock out')
                ORDER BY [Timestamp]
            ";

            List<Event> events = await EventServices.QueryEvents(query);
            List<HoursWperDayofWeek> weekHours = new List<HoursWperDayofWeek>();

            for (int i = 0; i < 7; i++)
            {
                DateTime currentDay = startDate.AddDays(i);
                DateTime nextDay = currentDay.AddDays(1);

                // Filter events for current day
                var dayEvents = events.Where(e =>
                    e.Timestamp >= currentDay &&
                    e.Timestamp < nextDay
                ).OrderBy(e => e.Timestamp).ToList();

                decimal hoursWorked = 0;

                // Find Clock in and Clock out pairs
                for (int j = 0; j < dayEvents.Count - 1; j++)
                {
                    if (dayEvents[j].EventType == "Clock in" && dayEvents[j + 1].EventType == "Clock out")
                    {
                        DateTime clockIn = dayEvents[j].Timestamp;
                        DateTime clockOut = dayEvents[j + 1].Timestamp;

                        // Calculate working minutes
                        double totalMinutes = (clockOut - clockIn).TotalMinutes;

                        // Round to nearest 5 minutes (F2-FR4)
                        totalMinutes = Math.Round(totalMinutes / 5.0) * 5.0;

                        // Convert to hours
                        decimal hours = (decimal)(totalMinutes / 60.0);

                        // Deduct 30 minutes unpaid break if shift > 5 hours (F2-FR3)
                        if (hours > 5)
                        {
                            hours -= 0.5m;
                        }

                        hoursWorked += hours;
                    }
                }

                weekHours.Add(new HoursWperDayofWeek
                {
                    DateofWeek = currentDay,
                    HoursWorked = hoursWorked
                });
            }

            return weekHours;
        }


        private static async Task<decimal> CalculatePayRollWeek(DateTime Monday_date, Staff staff)
        { 
            decimal TotalGrossPay = 0;

            //decimal StandardHours = staff.StandardHoursPerWeek != null ? (decimal)staff.StandardHoursPerWeek! : 38;
            // Assume that StandardHour always is 38.
            decimal StandardHours = 38;


            decimal PayRate = (decimal)staff.StandardPayRate!;

            // Step 1 & 2: Get weekly hours using private helper method
            List<HoursWperDayofWeek> weekHours = await CalculateWeeklyHours(Monday_date, staff.StaffId);

            // Step 3: Apply payroll calculation formula according to Feature 2
            decimal totalOrdinaryHours = 0;
            decimal dailyOvertimeHours = 0;
            decimal weekendHolidayHours = 0;
            decimal totalWeeklyHours = weekHours.Sum(h => h.HoursWorked);

            foreach (var day in weekHours)
            {
                decimal dayHours = day.HoursWorked;
                DayOfWeek dayOfWeek = day.DateofWeek.DayOfWeek;

                // Check if weekend (Saturday/Sunday)
                if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
                {
                    weekendHolidayHours += dayHours;
                }
                else
                {
                    // Calculate daily overtime (after 8 hours/day)
                    if (dayHours > 8)
                    {
                        dailyOvertimeHours += (dayHours - 8);
                        totalOrdinaryHours += 8;
                    }
                    else
                    {
                        totalOrdinaryHours += dayHours;
                    }
                }
            }

            // Calculate weekly overtime (after StandardHours/week, typically 38 hours)
            decimal weekdayHours = totalOrdinaryHours + dailyOvertimeHours;
            decimal weeklyOvertimeHours = 0;
            decimal weeklyOvertimeFirst2Hours = 0;
            decimal weeklyOvertimeAdditionalHours = 0;

            if (weekdayHours > StandardHours)
            {
                weeklyOvertimeHours = weekdayHours - StandardHours;
                
                // First 2 hours of overtime x 1.5
                if (weeklyOvertimeHours <= 2)
                {
                    weeklyOvertimeFirst2Hours = weeklyOvertimeHours;
                }
                else
                {
                    weeklyOvertimeFirst2Hours = 2;
                    weeklyOvertimeAdditionalHours = weeklyOvertimeHours - 2;
                }

                // Adjust ordinary hours
                totalOrdinaryHours = StandardHours;
            }

            // Step 4: Calculate Gross Pay
            // Ordinary hours
            TotalGrossPay += totalOrdinaryHours * PayRate;

            // Daily overtime (1.5x)
            TotalGrossPay += dailyOvertimeHours * (1.5m * PayRate);

            // Weekly overtime first 2 hours (1.5x)
            TotalGrossPay += weeklyOvertimeFirst2Hours * (1.5m * PayRate);

            // Weekly overtime additional hours (2.0x)
            TotalGrossPay += weeklyOvertimeAdditionalHours * (2.0m * PayRate);

            // Weekend/Holiday hours (2.0x)
            TotalGrossPay += weekendHolidayHours * (2.0m * PayRate);

            return TotalGrossPay;
        }


        // Step 2: Calculate Annualised Income
        // Annual Income = Gross Weekly Pay × 52
        private static decimal CalculateAnnualIncome(decimal grossWeeklyPay)
        {
            return grossWeeklyPay * 52;
        }


        // Step 3: Calculate PAYG Withholding (ignore Medicare levy)
        // Apply ATO resident tax thresholds
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


        // Calculate Weekly PAYG Withholding
        // Weekly PAYG = Annual Tax ÷ 52
        private static decimal CalculateWeeklyPAYG(decimal grossWeeklyPay)
        {
            decimal annualIncome = CalculateAnnualIncome(grossWeeklyPay);
            decimal annualTax = CalculateAnnualTax(annualIncome);
            return annualTax / 52;
        }


        // Step 4: Calculate Net Pay
        // Net Pay = Gross Pay – PAYG Withholding
        private static decimal CalculateNetPay(decimal grossWeeklyPay)
        {
            decimal weeklyPAYG = CalculateWeeklyPAYG(grossWeeklyPay);
            return grossWeeklyPay - weeklyPAYG;
        }


        // Step 5: Calculate Employer Superannuation
        // Superannuation = Gross Pay × Superannuation Guarantee (SG) Rate
        // Current SG = 12% from 1 July 2025
        private static decimal CalculateEmployerSuperannuation(decimal grossPay, decimal sgRate = 0.12m)
        {
            return grossPay * sgRate;
        }




 

    }
     
    
}
public class HoursWperDayofWeek
{
    public DateTime DateofWeek { get; set; }
    public decimal HoursWorked { get; set; }
}

public class PayrollSummary
{
    public int StaffId { get; set; }
    public string StaffName { get; set; } = null!;
    public DateTime WeekStartDate { get; set; }
    public decimal TotalHoursWorked { get; set; }
    public decimal GrossWeeklyPay { get; set; }
    public decimal AnnualIncome { get; set; }
    public decimal AnnualTax { get; set; }
    public decimal WeeklyPAYG { get; set; }
    public decimal NetPay { get; set; }
    public decimal EmployerSuperannuation { get; set; }
}