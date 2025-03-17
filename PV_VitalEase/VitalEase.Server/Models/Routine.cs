namespace VitalEase.Server.Models
{
    public class Routine
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public RoutineLevel Level { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public List<ScheduledRoutine> ScheduledRoutines { get; set; } = new();

        public ICollection<ExerciseRoutine> ExerciseRoutines { get; set; }
    }
}
