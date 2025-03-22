namespace VitalEase.Server.Models
{
    public class ExerciseMedia
    {
        public int ExerciseId { get; set; }
        public Exercise Exercise { get; set; } // Relacionamento com Exercise

        public int MediaId { get; set; }
        public Media Media { get; set; } // Relacionamento com Routine
    }
}
