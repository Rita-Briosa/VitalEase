namespace VitalEase.Server.Models
{
    public class ScheduledRoutine
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public ScheduleStatus Status { get; set; }

        public int RoutineId { get; set; }

        public Routine Routine { get; set; }

        public void MarkAsCompleted()
        {
            Status = ScheduleStatus.Completed;
        }

        public void Reschedule(DateTime newDate, TimeSpan newTime)
        {
            Date = newDate;
            Time = newTime;
        }

        public void CancelRoutine()
        {
            Status = ScheduleStatus.Pending;
        }

        public string GetRoutineDetails()
        {
            return $"Date: {Date}, Time: {Time}, Status: {Status}";
        }

        public bool IsOngoing()
        {
            return Status == ScheduleStatus.Ongoing;
        }
    }
}
