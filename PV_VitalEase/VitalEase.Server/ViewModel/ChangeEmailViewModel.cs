namespace VitalEase.Server.ViewModel
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents the view model used for updating a user's email address.
    /// </summary>
    /// <remarks>
    /// This view model contains the user's current email, the current password for authentication, and the new email address.
    /// All properties are mandatory, and the email properties are validated to ensure they are in a correct email format.
    /// </remarks>
    public class ChangeEmailViewModel
    {
        /// <summary>
        /// Gets or sets the user's current email address.
        /// </summary>
        /// <remarks>
        /// This field is required and must be a valid email address.
        /// </remarks>
        [Required(ErrorMessage = "The email is required")]
        [EmailAddress(ErrorMessage = "Email is invalid")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the user's current password.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The old password is required")]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the new email address.
        /// </summary>
        /// <remarks>
        /// This field is required and must be a valid email address.
        /// </remarks>
        [Required(ErrorMessage = "The new email is required")]
        [EmailAddress(ErrorMessage = "New Email is invalid")]
        public string NewEmail { get; set; }
    }
}
