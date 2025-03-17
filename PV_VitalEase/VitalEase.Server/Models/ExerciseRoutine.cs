namespace VitalEase.Server.Models
{
    public class ExerciseRoutine
    {
        public int ExerciseId { get; set; }
        public Exercise Exercise { get; set; } // Relacionamento com Exercise

        public int RoutineId { get; set; }
        public Routine Routine { get; set; } // Relacionamento com Routine
    }
}
