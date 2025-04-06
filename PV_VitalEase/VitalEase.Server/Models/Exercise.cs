namespace VitalEase.Server.Models
{
    /// <summary>
    /// Represents a physical exercise with detailed information and associations to routines and media.
    /// </summary>
    /// <remarks>
    /// This class encapsulates properties for identifying and describing an exercise, including its name, description,
    /// type, difficulty level, targeted muscle group, and the necessary equipment. It also maintains collections that
    /// associate the exercise with training routines and related media (e.g., images, videos).
    /// </remarks>
    public class Exercise
    {
        /// <summary>
        /// Gets or sets the unique identifier for the exercise.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the name of the exercise.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets a description that provides additional details about the exercise.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Gets or sets the type of the exercise (e.g., cardio, strength, flexibility).
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Gets or sets the difficulty level of the exercise, represented as a <see cref="RoutineLevel"/> enum.
        /// </summary>
        public RoutineLevel DifficultyLevel { get; set; }
        /// <summary>
        /// Gets or sets the primary muscle group targeted by the exercise.
        /// </summary>
        public string MuscleGroup { get; set; }
        /// <summary>
        /// Gets or sets the equipment necessary to perform the exercise.
        /// </summary>
        public string EquipmentNecessary { get; set; }
        /// <summary>
        /// Gets or sets the collection of <see cref="ExerciseRoutine"/> entities that associate this exercise with various training routines.
        /// </summary>
        public ICollection<ExerciseRoutine> ExerciseRoutine { get; set; }
        /// <summary>
        /// Gets or sets the collection of <see cref="ExerciseMedia"/> entities that represent media (such as images or videos) associated with the exercise.
        /// </summary>
        public ICollection<ExerciseMedia> ExerciseMedia { get; set; }

        /// <summary>
        /// Returns a string containing basic information about the exercise.
        /// </summary>
        /// <returns>A string in the format "Name: {Name}, Type: {Type}".</returns>
        public string GetExerciseInfo()
        {
            return $"Name: {Name}, Type: {Type}";
        }
    }

}
