using System.ComponentModel.DataAnnotations;

namespace VitalEase.Server.ViewModel
{
    /// <summary>
    /// Represents the view model used for updating a user's height.
    /// </summary>
    /// <remarks>
    /// This view model contains the user's email address and their height in centimeters. The email is required and must be in a valid email format,
    /// while the height is required and must be between 125 and 250 cm.
    /// </remarks>
    public class ChangeHeightViewModel
    {
        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        /// <remarks>
        /// This field is required and must be a valid email address.
        /// </remarks>
        [Required(ErrorMessage = "The email is required")]
        [EmailAddress(ErrorMessage = "Email is invalid")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the user's height in centimeters.
        /// </summary>
        /// <remarks>
        /// This field is required and must be between 90 and 300 cm.
        /// </remarks>
        [Range(90, 300, ErrorMessage = "Height must be between 90 and 300 cm.")]
        [Required(ErrorMessage = "Height is required")]
        public int Height { get; set; }
    }
}
