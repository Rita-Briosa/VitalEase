namespace VitalEase.Server.ViewModel
{
    /// <summary>
    /// Represents the view model used for editing an exercise routine.
    /// </summary>
    /// <remarks>
    /// This view model includes optional properties for specifying the number of repetitions, the duration, and the number of sets for an exercise routine.
    /// </remarks>
    public class EditExerciseRoutineViewModel
    {
        /// <summary>
        /// Gets or sets the number of repetitions.
        /// </summary>
        public int? reps { get; set; }

        /// <summary>
        /// Gets or sets the duration, typically expressed in seconds.
        /// </summary>
        public int? duration { get; set; }

        /// <summary>
        /// Gets or sets the number of sets.
        /// </summary>
        public int? sets { get; set; }
    }
}
