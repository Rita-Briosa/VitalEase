namespace VitalEase.Server.ViewModel
{
    using System.ComponentModel.DataAnnotations;
    using VitalEase.Server.Models;

    /// <summary>
    /// Represents the view model used for adding new exercises.
    /// </summary>
    /// <remarks>
    /// This view model includes properties that describe the details of an exercise, such as its name, description, type, difficulty level, muscle group, required equipment,
    /// and associated media information. Some media properties are mandatory while others are optional.
    /// </remarks>
    public class AddExercisesViewModel
    {
        /// <summary>
        /// Gets or sets the new name of the exercise.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The new Name is required")]
        public string newName { get; set; }

        /// <summary>
        /// Gets or sets the new description of the exercise.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The new Description is required")]
        public string newDescription { get; set; }

        /// <summary>
        /// Gets or sets the new type of the exercise.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The new Type is required")]
        public string newType{ get; set; }

        /// <summary>
        /// Gets or sets the new difficulty level of the exercise.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The new Difficulty Level is required")]
        public string newDifficultyLevel { get; set; }

        /// <summary>
        /// Gets or sets the new muscle group targeted by the exercise.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The new Muscle Group is required")]
        public string newMuscleGroup { get; set; }

        /// <summary>
        /// Gets or sets the required equipment for the exercise.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The new Equipment Necessary is required")]
        public string newEquipmentNecessary { get; set; }

        /// <summary>
        /// Gets or sets the name of the primary media associated with the exercise.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The new Media Name is required")]
        public string newMediaName { get; set; }

        /// <summary>
        /// Gets or sets the type of the primary media associated with the exercise.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The new Media Type is required")]
        public string newMediaType { get; set; }

        /// <summary>
        /// Gets or sets the URL of the primary media associated with the exercise.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The new Media Url is required")]
        public string newMediaUrl { get; set; }

        /// <summary>
        /// Gets or sets the name of the secondary media associated with the exercise.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The new Media Name 1 is required")]
        public string newMediaName1 { get; set; }

        /// <summary>
        /// Gets or sets the type of the secondary media associated with the exercise.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The new Media Type 1 is required")]
        public string newMediaType1 { get; set; }

        /// <summary>
        /// Gets or sets the URL of the secondary media associated with the exercise.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The new Media Url 1 is required")]
        public string newMediaUrl1 { get; set; }

        /// <summary>
        /// Gets or sets the name of the tertiary media associated with the exercise.
        /// </summary>
        /// <remarks>
        /// This field is optional.
        /// </remarks>
        public string? newMediaName2 { get; set; }

        /// <summary>
        /// Gets or sets the type of the tertiary media associated with the exercise.
        /// </summary>
        /// <remarks>
        /// This field is optional.
        /// </remarks>
        public string? newMediaType2 { get; set; }

        /// <summary>
        /// Gets or sets the URL of the tertiary media associated with the exercise.
        /// </summary>
        /// <remarks>
        /// This field is optional.
        /// </remarks>
        public string? newMediaUrl2 { get; set; }

    }
}
