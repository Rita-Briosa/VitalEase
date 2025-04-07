namespace VitalEase.Server.Models
{
    /// <summary>
    /// Represents an audit log entry containing information about an action performed by a user.
    /// </summary>
    public class AuditLog
    {
        /// <summary>
        /// Gets or sets the unique identifier of the audit log entry.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the action was logged.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the description of the performed action.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the status or outcome of the action.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user associated with the action.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Logs the current action by setting the description, the status, and updating the timestamp to the current moment.
        /// </summary>
        /// <param name="action">The description of the action to log.</param>
        /// <param name="status">The status or outcome of the action.</param>
        public void LogAction(string action, string status)
        {
            Action = action;
            Status = status;
            Timestamp = DateTime.Now;
        }

        /// <summary>
        /// Filters a list of audit logs, returning only those whose <c>Action</c> property matches the specified value.
        /// </summary>
        /// <param name="action">The action by which to filter the logs.</param>
        /// <param name="logs">The list of audit logs to be filtered.</param>
        /// <returns>
        /// A list of audit logs that have the specified value in the <c>Action</c> property.
        /// </returns>
        public static List<AuditLog> FilterLogsByAction(string action, List<AuditLog> logs)
        {
            return logs.FindAll(log => log.Action == action);
        }

        /// <summary>
        /// Retrieves all audit logs.
        /// </summary>
        /// <returns>
        /// A list containing all audit logs.
        /// </returns>
        /// <remarks>
        /// This method is currently not fully implemented and returns a new empty list.
        /// </remarks>
        public static List<AuditLog> GetAllLogs()
        {
            // Implementation here
            return new List<AuditLog>();
        }
    }
}
