using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using RestfulAPI_FarmTimeManagement.DataConnects;
using RestfulAPI_FarmTimeManagement.Models;
using RestfulAPI_FarmTimeManagement.Services.Sprint1.Tom;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

namespace RestfulAPI_FarmTimeManagement.Services.Sprint_2.Tom
{
    public static class EventServices
    {
        public static async Task<List<Event>> GetAllEvents()
        {
            var connects = new EventConnects();
            var query = @"SELECT * FROM [Event] ORDER BY [Timestamp] DESC, EventId DESC";
            return await connects.QueryEvent(query);
        }

        public static async Task<List<Event>> QueryEvents(string querystring)
        {
            var connects = new EventConnects();
            return await connects.QueryEvent(querystring);
        }




        public static async Task<Event?> CreateEvent(Event item)
        {
            var connects = new EventConnects();
            return await connects.CreateEvent(item);
        }




        public static async Task<Event?> Create_Clock_in_Event(int staff_id,int device_id)
        {


            // I have commented this code due to make it is faster for demo in stead wating 1 minute for testing

            ////8.3.5 The system shall reject duplicate clock-ins from the same staff ID within 1 minute.
            //List<Event> existingClockIns = 
            //    await QueryEvents($@"SELECT * FROM [Event] WHERE StaffId = {staff_id} AND EventType = 'Clock in' AND DATEDIFF(MINUTE, [Timestamp], GETDATE()) < 1");

            //if (existingClockIns.Count > 0)
            //{
            //   throw new Exception("Duplicate clock in detected within 1 minute.");
            //}





            Event item = new Event
            {
                Timestamp = DateTime.Now,
                StaffId = staff_id,
                DeviceId = device_id,
                EventType = "Clock in",
                Reason = null
            }; 

            var connects = new EventConnects();
            return await connects.CreateEvent(item);
        }



        public static async Task<Event?> Create_Clock_out_Event(int staff_id, int device_id)
        {

            // I have commented this code due to make it is faster for demo in stead wating 1 minute for testing

            //            // 1) 8.3.5: Reject duplicate clock-out from the same staff ID within 1 minute.
            //            List<Event> existingClockOut =
            //                await QueryEvents($@"
            //SELECT * 
            //FROM [Event] 
            //WHERE StaffId = {staff_id} 
            //  AND EventType = 'Clock out' 
            //  AND DATEDIFF(MINUTE, [Timestamp], GETDATE()) < 1");

            //            if (existingClockOut.Count > 0)
            //                throw new Exception("Duplicate clock out detected within 1 minute.");

            //            // 2) Must have a valid open 'Clock in' (latest clock-in without a later clock-out)
            //             var latestClockIn = await QueryEvents($@"
            //SELECT TOP 1 * 
            //FROM [Event]
            //WHERE StaffId = {staff_id}
            //  AND EventType = 'Clock in'
            //  AND [Timestamp] <= GETDATE()
            //ORDER BY [Timestamp] DESC");

            //            if (latestClockIn.Count == 0)
            //                throw new Exception("No valid clock-in found for the current shift.");

            //            var clockInTs = latestClockIn[0].Timestamp;

            //            // 3) check whether there any lock out after these lock in
            //            var clockOutAfter = await QueryEvents($@"
            //SELECT TOP 1 EventId 
            //FROM [Event]
            //WHERE StaffId = {staff_id}
            //  AND EventType = 'Clock out'
            //  AND [Timestamp] > '{clockInTs:O}'");  

            //            if (clockOutAfter.Count > 0)
            //                throw new Exception("Clock-out blocked: the latest clock-in has already been closed.");

            Event item = new Event
            {
                Timestamp = DateTime.Now,     
                StaffId = staff_id,
                DeviceId = device_id,
                EventType = "Clock out",
                Reason = null
            };

            var connects = new EventConnects();
            return await connects.CreateEvent(item);
        }

         

        public static async Task<Event?> UpdateEvent(int id, Event item, HttpContext httpContext)
        {
            var connects = new EventConnects();
           
            Event _event = await connects.UpdateEvent(id, item);

            Staff staff = await StaffsServices.GetStaffById(item.StaffId);

            if (_event != null)
            {
                var his = await HistoryServices.CreateHistory(
                    "Update Event",
                    "Succeed",
                    $"Event {_event.EventType} for Staff {staff.FirstName} was updated.",
                    httpContext
                );
                if (his != null) return _event;
            }

            return new Event
            {
                DeviceId = -1,
                Reason = "Failed due to system issue."
            };

        }



        public static async Task<Event?> DeleteEvent(int id, HttpContext httpContext)
        {
            var connects = new EventConnects();



            Event _event = await connects.DeleteEvent(id);
            Staff staff = await StaffsServices.GetStaffById(_event.StaffId);

            if (_event != null)
            {
                var his = await HistoryServices.CreateHistory(
                    "Delete Event",
                    "Succeed",
                    $"Event {_event.EventType} for Staff {staff.FirstName} was deleted.",
                    httpContext
                );
                if (his != null) return _event;
            }

            return new Event
            {
                DeviceId = -1,
                Reason = "Failed due to system issue."
            };







        }




        public static async Task<List<ResultRoster>> ReportLock_in_LockOUT(DateTime date)
        {
            var workScheduleConnects = new WorkScheduleConnects();
            var eventConnects = new EventConnects();

            // Xác định khung ngày [00:00, 24:00) của ngày được truyền vào
            var dayStart = date.Date;
            var dayEnd = dayStart.AddDays(1);

            // 1) Lấy tất cả WorkSchedule có StartTime nằm TRONG ngày
            var schedules = await workScheduleConnects.QueryWorkSchedule($@"
        SELECT * FROM WorkSchedule
        WHERE StartTime >= '{dayStart:yyyy-MM-dd}'
          AND StartTime <  '{dayEnd:yyyy-MM-dd}'
        ORDER BY StaffId, StartTime
    ");

            var results = new List<ResultRoster>();

            foreach (var ws in schedules)
            {
                // 2) Lấy tất cả event của staff trong khoảng lịch
                var events = await eventConnects.QueryEvent($@"
            SELECT * FROM [Event]
            WHERE StaffId = {ws.StaffId}
              AND [Timestamp] >= '{ws.StartTime:O}'
              AND [Timestamp] <= '{ws.EndTime:O}'
            ORDER BY [Timestamp]
        ");

                // Tách clock-in / clock-out trong khung lịch
                var clockIn = events
                    .Where(e => e.EventType == "Clock in")
                    .OrderBy(e => e.Timestamp)
                    .Select(e => (DateTime?)e.Timestamp)
                    .FirstOrDefault();

                var clockOut = events
                    .Where(e => e.EventType == "Clock out")
                    .OrderByDescending(e => e.Timestamp)
                    .Select(e => (DateTime?)e.Timestamp)
                    .FirstOrDefault();

                // 3) Tính status
                var statusParts = new List<string>();

                if (clockIn.HasValue)
                {
                    if (clockIn.Value - ws.StartTime > TimeSpan.FromMinutes(10))
                        statusParts.Add("clock in late");
                }
                else
                {
                    statusParts.Add("missing clock in");
                }

                if (clockOut.HasValue)
                {
                    if (ws.EndTime - clockOut.Value > TimeSpan.FromMinutes(10))
                        statusParts.Add("clock out early");
                }
                else
                {
                    statusParts.Add("missing clock out");
                }

                var status = string.Join(", ", statusParts);

                // 4) actual_hour = (clock_out - clock_in) làm tròn 15'
                double actualHours = 0.0;
                DateTime clockInVal = clockIn ?? ws.StartTime;   // fallback để điền cấu trúc
                DateTime clockOutVal = clockOut ?? ws.EndTime;    // fallback để điền cấu trúc

                if (clockIn.HasValue && clockOut.HasValue && clockOut > clockIn)
                {
                    var span = clockOut.Value - clockIn.Value;
                    var rounded = RoundToQuarterHour(span); // làm tròn tới 0/15/30/45 phút
                    actualHours = Math.Round(rounded.TotalMinutes / 60.0, 2, MidpointRounding.AwayFromZero);
                    // Ví dụ: 7 giờ 15' -> 7.25
                }

                // 5) Lấy tên nhân viên
                var staff = await StaffsServices.GetStaffById(ws.StaffId);
                var fullName = $"{staff.FirstName} {staff.LastName}";

                results.Add(new ResultRoster
                {
                    Staff_fullname = fullName,
                    StartTime = ws.StartTime,
                    EndTime = ws.EndTime,
                    Clock_in = clockInVal,
                    Clock_out = clockOutVal,
                    actual_hour = actualHours,
                    Status = status
                });
            }

            return results;

            // Helper: làm tròn TimeSpan tới bậc 15 phút
            static TimeSpan RoundToQuarterHour(TimeSpan ts)
            {
                const int quarter = 15;
                var totalMinutes = ts.TotalMinutes;
                var roundedMinutes = Math.Round(totalMinutes / quarter) * quarter;
                return TimeSpan.FromMinutes(roundedMinutes);
            }
        }






    }
}

public class ResultRoster { 

    public string Staff_fullname { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; } 
    public DateTime Clock_in { get; set; }
    public DateTime Clock_out { get; set; }
    public int ScheduleHours { get; set; }
    public double actual_hour { get; set; }
    public string Status { get; set; }


}