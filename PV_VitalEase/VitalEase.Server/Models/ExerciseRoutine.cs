namespace VitalEase.Server.Models
{
    /// <summary>
    /// Represents the association between an exercise and a training routine,
    /// including details on repetitions, sets, and duration for the exercise within the routine.
    /// </summary>
    /// <remarks>
    /// This class models the many-to-many relationship between <see cref="Exercise"/> and <see cref="Routine"/>.
    /// It allows specifying additional parameters for how an exercise is performed in a particular routine.
    /// </remarks>
    public class ExerciseRoutine
    {
        /// <summary>
        /// Gets or sets the identifier of the associated exercise.
        /// </summary>
        public int ExerciseId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Exercise"/> entity associated with this relationship.
        /// </summary>
        /// <remarks>
        /// This property establishes the link to the exercise that is part of the routine.
        /// </remarks>
        public Exercise Exercise { get; set; } // Relacionamento com Exercise

        /// <summary>
        /// Gets or sets the identifier of the associated routine.
        /// </summary>
        public int RoutineId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Routine"/> entity associated with this relationship.
        /// </summary>
        /// <remarks>
        /// This property establishes the link to the training routine that includes the exercise.
        /// </remarks>
        public Routine Routine { get; set; } // Relacionamento com Routine

        /// <summary>
        /// Gets or sets the number of repetitions to perform for the exercise in the routine.
        /// </summary>
        /// <remarks>
        /// This property is optional and may be null if not applicable.
        /// </remarks>
        public int? Reps { get; set; }

        /// <summary>
        /// Gets or sets the number of sets to perform for the exercise in the routine.
        /// </summary>
        /// <remarks>
        /// This property is optional and may be null if not applicable.
        /// </remarks>
        public int? Sets { get; set; }

        /// <summary>
        /// Gets or sets the duration (in seconds) for which the exercise should be performed in the routine.
        /// </summary>
        /// <remarks>
        /// This property is optional and may be null if the exercise is not time-based.
        /// </remarks>
        public int? Duration { get; set; } // in seconds
    }
}
