namespace VitalEase.Server.Models
{
    /// <summary>
    /// Represents a training routine with associated details such as name, description, type, difficulty level, and custom settings.
    /// </summary>
    /// <remarks>
    /// A <c>Routine</c> can be either a global routine (if <c>UserId</c> is null) or a custom routine created by a specific user.
    /// It also holds collections of scheduled routines and exercise associations that define the structure and content of the routine.
    /// </remarks>
    public class Routine
    {
        /// <summary>
        /// Gets or sets the unique identifier for the routine.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the routine.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description that provides additional details about the routine.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the type of the routine (e.g., strength, cardio, flexibility).
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the difficulty level of the routine, represented by the <see cref="RoutineLevel"/> enum.
        /// </summary>
        public RoutineLevel Level { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who created the routine, if applicable.
        /// </summary>
        /// <remarks>
        /// This property is nullable. If null, the routine is considered a global routine.
        /// </remarks>
        public int? UserId { get; set; }

        /// <summary>
        /// Gets or sets the user associated with the routine.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the routine is custom (created by a user) or a predefined global routine.
        /// </summary>
        public bool IsCustom { get; set; } // Boolean para saber se é personalizada

        /// <summary>
        /// Gets or sets specific needs or requirements for the routine.
        /// </summary>
        /// <remarks>
        /// This property can be used to specify any special equipment or conditions required to perform the routine.
        /// </remarks>
        public string Needs { get; set; } // Pode ser uma string para necessidades específicas

        /// <summary>
        /// Gets or sets the collection of scheduled routines associated with this routine.
        /// </summary>
        public List<ScheduledRoutine> ScheduledRoutines { get; set; } = new();

        /// <summary>
        /// Gets or sets the collection of exercise associations that define which exercises are included in this routine.
        /// </summary>
        public ICollection<ExerciseRoutine> ExerciseRoutine { get; set; }
    }
}
