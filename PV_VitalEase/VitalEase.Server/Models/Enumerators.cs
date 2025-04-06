namespace VitalEase.Server.Models
{
    /// <summary>
    /// Represents the difficulty level of a training routine.
    /// </summary>
    public enum RoutineLevel
    {
        /// <summary>
        /// Indicates a beginner-level routine.
        /// </summary>
        Beginner,
        /// <summary>
        /// Indicates an intermediate-level routine.
        /// </summary>
        Intermediate,
        /// <summary>
        /// Indicates an advanced-level routine.
        /// </summary>
        Advanced
    }

    /// <summary>
    /// Represents the status of a schedule.
    /// </summary>
    public enum ScheduleStatus
    {
        /// <summary>
        /// Indicates that the scheduled task has been completed.
        /// </summary>
        Completed,
        /// <summary>
        /// Indicates that the scheduled task is currently ongoing.
        /// </summary>
        Ongoing,
        /// <summary>
        /// Indicates that the scheduled task is pending.
        /// </summary>
        Pending
    }

    /// <summary>
    /// Represents the type of a user in the system.
    /// </summary>
    public enum UserType
    {
        /// <summary>
        /// Represents a standard user with regular access privileges.
        /// </summary>
        Standard,
        /// <summary>
        /// Represents an administrator with elevated access privileges.
        /// </summary>
        Admin
    }

}
