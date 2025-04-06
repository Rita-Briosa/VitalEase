namespace VitalEase.Server.Models
{
    /// <summary>
    /// Represents the association between an exercise and a media item.
    /// </summary>
    /// <remarks>
    /// This class is used to model the many-to-many relationship between <see cref="Exercise"/> and <see cref="Media"/>.
    /// It allows linking multiple media items (e.g., images, videos) to an exercise, and vice versa.
    /// </remarks>
    public class ExerciseMedia
    {
        /// <summary>
        /// Gets or sets the identifier of the associated exercise.
        /// </summary>
        public int ExerciseId { get; set; }

        /// <summary>
        /// Gets or sets the associated <see cref="Exercise"/> entity.
        /// </summary>
        /// <remarks>
        /// This property represents the relationship to the <see cref="Exercise"/> that this media is linked to.
        /// </remarks>
        public Exercise Exercise { get; set; } // Relacionamento com Exercise

        /// <summary>
        /// Gets or sets the identifier of the associated media item.
        /// </summary>
        public int MediaId { get; set; }

        /// <summary>
        /// Gets or sets the associated <see cref="Media"/> entity.
        /// </summary>
        /// <remarks>
        /// This property represents the relationship to the <see cref="Media"/> item that is linked to the exercise.
        /// </remarks>
        public Media Media { get; set; } // Relacionamento com Routine
    }
}
