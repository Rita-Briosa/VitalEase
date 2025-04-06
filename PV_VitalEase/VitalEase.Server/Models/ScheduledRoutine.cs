namespace VitalEase.Server.Models
{
    /// <summary>
    /// Represents a scheduled instance of a training routine, including its scheduled date, time, and current status.
    /// </summary>
    /// <remarks>
    /// This class encapsulates the scheduling details for a routine and provides methods to update its status,
    /// reschedule, cancel, retrieve its details, and check if it is ongoing.
    /// </remarks>
    public class ScheduledRoutine
    {
        /// <summary>
        /// Gets or sets the unique identifier for the scheduled routine.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the date on which the routine is scheduled.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the time at which the routine is scheduled.
        /// </summary>
        public TimeSpan Time { get; set; }

        /// <summary>
        /// Gets or sets the current status of the scheduled routine.
        /// </summary>
        /// <remarks>
        /// The status is represented by the <see cref="ScheduleStatus"/> enum.
        /// </remarks>
        public ScheduleStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the associated routine.
        /// </summary>
        public int RoutineId { get; set; }

        /// <summary>
        /// Gets or sets the associated <see cref="Routine"/> entity.
        /// </summary>
        public Routine Routine { get; set; }

        /// <summary>
        /// Marks the scheduled routine as completed by setting its status to <see cref="ScheduleStatus.Completed"/>.
        /// </summary>
        public void MarkAsCompleted()
        {
            Status = ScheduleStatus.Completed;
        }

        /// <summary>
        /// Reschedules the routine by updating its date and time.
        /// </summary>
        /// <param name="newDate">The new date for the routine.</param>
        /// <param name="newTime">The new time for the routine.</param>
        public void Reschedule(DateTime newDate, TimeSpan newTime)
        {
            Date = newDate;
            Time = newTime;
        }

        /// <summary>
        /// Cancels the routine by setting its status to <see cref="ScheduleStatus.Pending"/>.
        /// </summary>
        public void CancelRoutine()
        {
            Status = ScheduleStatus.Pending;
        }

        /// <summary>
        /// Retrieves a summary of the routine's scheduling details.
        /// </summary>
        /// <returns>A string containing the date, time, and current status of the routine.</returns>
        public string GetRoutineDetails()
        {
            return $"Date: {Date}, Time: {Time}, Status: {Status}";
        }

        /// <summary>
        /// Determines whether the routine is currently ongoing.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the routine's status is <see cref="ScheduleStatus.Ongoing"/>; otherwise, <c>false</c>.
        /// </returns>
        public bool IsOngoing()
        {
            return Status == ScheduleStatus.Ongoing;
        }
    }
}
