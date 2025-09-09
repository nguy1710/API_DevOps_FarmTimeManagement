using RestfulAPI_FarmTimeManagement.DataConnects;
using RestfulAPI_FarmTimeManagement.Models;

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
            //8.3.5 The system shall reject duplicate clock-ins from the same staff ID within 1 minute.
            List<Event> existingClockIns = 
                await QueryEvents($@"SELECT * FROM [Event] WHERE StaffId = {staff_id} AND EventType = 'Clock in' AND DATEDIFF(MINUTE, [Timestamp], GETDATE()) < 1");

            if (existingClockIns.Count > 0)
            {
               throw new Exception("Duplicate clock in detected within 1 minute.");
            }

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
            // 1) 8.3.5: Reject duplicate clock-out from the same staff ID within 1 minute.
            List<Event> existingClockOut =
                await QueryEvents($@"
SELECT * 
FROM [Event] 
WHERE StaffId = {staff_id} 
  AND EventType = 'Clock out' 
  AND DATEDIFF(MINUTE, [Timestamp], GETDATE()) < 1");

            if (existingClockOut.Count > 0)
                throw new Exception("Duplicate clock out detected within 1 minute.");

            // 2) Must have a valid open 'Clock in' (latest clock-in without a later clock-out)
             var latestClockIn = await QueryEvents($@"
SELECT TOP 1 * 
FROM [Event]
WHERE StaffId = {staff_id}
  AND EventType = 'Clock in'
  AND [Timestamp] <= GETDATE()
ORDER BY [Timestamp] DESC");

            if (latestClockIn.Count == 0)
                throw new Exception("No valid clock-in found for the current shift.");

            var clockInTs = latestClockIn[0].Timestamp;

            // 3) check whether there any lock out after these lock in
            var clockOutAfter = await QueryEvents($@"
SELECT TOP 1 EventId 
FROM [Event]
WHERE StaffId = {staff_id}
  AND EventType = 'Clock out'
  AND [Timestamp] > '{clockInTs:O}'");  

            if (clockOutAfter.Count > 0)
                throw new Exception("Clock-out blocked: the latest clock-in has already been closed.");

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








        public static async Task<Event?> UpdateEvent(int id, Event item)
        {
            var connects = new EventConnects();
            return await connects.UpdateEvent(id, item);
        }

        public static async Task<Event?> DeleteEvent(int id)
        {
            var connects = new EventConnects();
            return await connects.DeleteEvent(id);
        }
    }
}
