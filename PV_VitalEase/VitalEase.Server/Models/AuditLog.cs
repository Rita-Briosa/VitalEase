namespace VitalEase.Server.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
        public string Status { get; set; }
        public int UserId { get; set; }

        public void LogAction(string action, string status)
        {
            Action = action;
            Status = status;
            Timestamp = DateTime.Now;
        }

        public static List<AuditLog> FilterLogsByAction(string action, List<AuditLog> logs)
        {
            return logs.FindAll(log => log.Action == action);
        }

        public static List<AuditLog> GetAllLogs()
        {
            // Implementation here
            return new List<AuditLog>();
        }
    }
}
