using System.ComponentModel.DataAnnotations;

namespace VitalEase.Server.ViewModel
{
    /// <summary>
    /// Represents the view model used for updating a user's weight.
    /// </summary>
    /// <remarks>
    /// This view model contains the user's email address and their weight in kilograms.
    /// The email field is required and must be a valid email address, while the weight field is required and must be between 30 and 250 kg.
    /// </remarks>
    public class ChangeWeightViewModel
    {
        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        /// <remarks>
        /// This field is required and must be in a valid email format.
        /// </remarks>
        [Required(ErrorMessage = "The email is required")]
        [EmailAddress(ErrorMessage = "Email is invalid")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the user's weight in kilograms.
        /// </summary>
        /// <remarks>
        /// This field is required and must be between 30 and 250 kg.
        /// </remarks>
        [Required(ErrorMessage = "Weight is required")]
        [Range(30, 450, ErrorMessage = "Weight must be between 30 and 450 kg.")]
        public int Weight { get; set; }
    }
}
