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
        

        public static async Task<List<Event>> QueryEvents(string querystring)
        {
            var connects = new EventConnects();
            return await connects.QueryEvent(querystring);
        }

        /// <summary>
        /// Gets events for a specific staff member with optional week filtering
        /// </summary>
        public static async Task<List<Event>> GetEventsByStaffId(int staffId, DateTime? weekStartDate = null)
        {
            var connects = new EventConnects();
            
            if (weekStartDate.HasValue)
            {
                // Calculate the start and end of the week (Monday to Sunday)
                var weekStart = GetWeekStart(weekStartDate.Value);
                var weekEnd = weekStart.AddDays(7);
                
                var query = $@"
                    SELECT * FROM Event 
                    WHERE StaffId = {staffId}
                    AND Timestamp >= '{weekStart:yyyy-MM-dd 00:00:00}'
                    AND Timestamp < '{weekEnd:yyyy-MM-dd 00:00:00}'
                    ORDER BY Timestamp";
                
                return await connects.QueryEvent(query);
            }
            else
            {
                return await connects.QueryEvent($"SELECT * FROM Event WHERE StaffId = {staffId} ORDER BY Timestamp");
            }
        }

        /// <summary>
        /// Gets the start of the week (Monday) for a given date
        /// </summary>
        private static DateTime GetWeekStart(DateTime date)
        {
            var daysSinceMonday = ((int)date.DayOfWeek - 1 + 7) % 7;
            return date.Date.AddDays(-daysSinceMonday);
        }




        public static async Task<Event?> CreateEvent(Event item)
        {
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




 





    }
}

 