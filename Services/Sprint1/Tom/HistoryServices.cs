using RestfulAPI_FarmTimeManagement.Models;
using RestfulAPI_FarmTimeManagement.DataConnects;
using System.Net;

namespace RestfulAPI_FarmTimeManagement.Services.Sprint1.Tom
{
    public static class HistoryServices
    {



        public static async Task<List<History>> GetAllHistories()
        {
            HistoryConnects HistoryConnects = new HistoryConnects();
            var querystring = $@"SELECT * FROM History ORDER BY [Timestamp] DESC, HistoryId DESC";
            List<History> result = await HistoryConnects.QueryHistory(querystring);
            return result;
        }



        public static async Task<List<History>> QueryHistories(string querystring)
        {
            HistoryConnects HistoryConnects = new HistoryConnects();
            List<History> result = await HistoryConnects.QueryHistory(querystring);
            return result;
        }




        public static async Task<History> CreateHistory(History history)
        {
            HistoryConnects HistoryConnects = new HistoryConnects();
             

            history.Timestamp = DateTime.UtcNow;
            history.Ip = Config.client_ip;


            var result = await HistoryConnects.CreateHistory(history);
            return result;
        }




        public static async Task<History> DeleteHistory (int id)
        {
            HistoryConnects HistoryConnects = new HistoryConnects();
            var result = await HistoryConnects.DeleteHistory(id);
            return result;
        }
         
         


        //Get IP from client.
        public static string? GetClientIp(HttpContext ctx)
        {

            // Ưu tiên header khi đứng sau proxy/CDN
            var xRealIp = ctx.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(xRealIp))
                return xRealIp.Split(',')[0].Trim();

            var xff = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(xff))
                return xff.Split(',')[0].Trim();

            // Lấy IP trực tiếp từ kết nối
            var addr = ctx.Connection.RemoteIpAddress;
            if (addr == null) return null;

            // Nếu là IPv4-mapped IPv6 thì map về IPv4
            if (addr.IsIPv4MappedToIPv6)
                addr = addr.MapToIPv4();

            // Nếu là loopback (::1 hoặc 127.0.0.1) -> tuỳ chọn:
            //   a) Trả "127.0.0.1" để dễ đọc
            //   b) Hoặc trả null để khỏi lưu vào DB
            if (IPAddress.IsLoopback(addr))
                return "127.0.0.1"; // hoặc: return null;

            return addr.ToString();
        }







    }
}
