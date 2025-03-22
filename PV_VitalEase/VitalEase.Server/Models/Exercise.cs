namespace VitalEase.Server.Models
{
    public class Exercise
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }

        public RoutineLevel DifficultyLevel { get; set; }

        public string MuscleGroup { get; set; }

        public string EquipmentNecessary { get; set; }

        public int Reps { get; set; }
        public int Duration { get; set; } // in seconds

        public ICollection<ExerciseRoutine> ExerciseRoutine { get; set; }

        public ICollection<ExerciseMedia> ExerciseMedia { get; set; }
  


        public string GetExerciseInfo()
        {
            return $"Name: {Name}, Type: {Type}, Reps: {Reps}, Duration: {Duration}";
        }
    }

}
