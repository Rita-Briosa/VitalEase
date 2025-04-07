using System.ComponentModel.DataAnnotations;
using VitalEase.Server.Models;

namespace VitalEase.Server.ViewModel
{
    /// <summary>
    /// Represents a view model for adding an exercise to a routine.
    /// </summary>
    public class AddRoutineFromExercisesViewModel
    {
        /// <summary>
        /// Gets or sets the identifier of the exercise.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The Exercise Id is required")]
        public int ExerciseId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the routine.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The Routine Id is required")]
        public int RoutineId { get; set; }

        /// <summary>
        /// Gets or sets the number of repetitions for the exercise.
        /// </summary>
        /// <remarks>
        /// This field is optional.
        /// </remarks>
        public int? reps { get; set; }

        /// <summary>
        /// Gets or sets the duration of the exercise, typically in seconds.
        /// </summary>
        /// <remarks>
        /// This field is optional.
        /// </remarks>
        public int? duration { get; set; }

        /// <summary>
        /// Gets or sets the number of sets for the exercise.
        /// </summary>
        /// <remarks>
        /// This field is optional.
        /// </remarks>
        public int? sets { get; set; }
    }
}
