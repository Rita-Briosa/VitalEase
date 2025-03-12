namespace VitalEase.Server.Models
{
    public class Routine
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public RoutineLevel Level { get; set; }

        
        public List<Exercise> Exercises { get; set; } = new();

        public int CalculateTotalDuration()
        {
            int totalDuration = 0;
            foreach (var exercise in Exercises)
            {
                totalDuration += exercise.Duration;
            }
            return totalDuration;
        }

        public string GetRoutineInfo()
        {
            return $"Name: {Name}, Level: {Level}, Total Duration: {CalculateTotalDuration()} seconds";
        }

        public void AddExercise(Exercise exercise)
        {
            Exercises.Add(exercise);
        }

        public void RemoveExercise(Exercise exercise)
        {
            Exercises.Remove(exercise);
        }
        
    }
}
