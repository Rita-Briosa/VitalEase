using System.ComponentModel.DataAnnotations;

namespace VitalEase.Server.ViewModel
{
    /// <summary>
    /// Represents the view model used for adding a new routine.
    /// </summary>
    /// <remarks>
    /// This view model contains properties for specifying the details of a routine, such as its name, description, type, routine level, and specific needs.
    /// It also includes a list of exercise IDs that are associated with the routine.
    /// </remarks>
    public class AddRoutineViewModel
    {
        /// <summary>
        /// Gets or sets the new name of the routine.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The new Name is required")]
        public string newName { get; set; }

        /// <summary>
        /// Gets or sets the new description of the routine.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The new Description is required")]
        public string newDescription { get; set; }

        /// <summary>
        /// Gets or sets the new type of the routine.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The new Type is required")]
        public string newType { get; set; }

        /// <summary>
        /// Gets or sets the new routine level.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The new Routine Level is required")]
        public string newRoutineLevel { get; set; }

        /// <summary>
        /// Gets or sets the new needs associated with the routine.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The new needs is required")]
        public string newNeeds { get; set; }

        /// <summary>
        /// Gets or sets the list of exercise IDs selected for the routine.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The exercises is required")]
        public List<int> Exercises { get; set; } // IDs dos exercícios selecionados

    }
}
