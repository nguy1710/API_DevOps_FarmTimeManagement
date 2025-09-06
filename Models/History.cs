namespace RestfulAPI_FarmTimeManagement.Models
{
    public class History
    {
        public int HistoryId { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Actor { get; set; }   // Username,email / tên đối tượng
        public string? Ip { get; set; }      // .....
        public string? Action { get; set; }  // login, logout, Rollin/rollout...
        public string? Result { get; set; }  // success, failure
        public string? Details { get; set; } // thêm mô tả chi tiết
    }
}
