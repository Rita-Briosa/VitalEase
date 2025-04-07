namespace VitalEase.Server.ViewModel
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents the view model used for updating a user's password.
    /// </summary>
    /// <remarks>
    /// This view model contains the user's email address, the current (old) password, and the new password. 
    /// All fields are required, with the email field being validated to ensure it is in a proper email format.
    /// </remarks>
    public class ChangePasswordViewModel
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
        /// Gets or sets the user's current (old) password.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The old password is required")]
        public string OldPassword { get; set; }

        /// <summary>
        /// Gets or sets the user's new password.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The new password is required")]
        public string NewPassword { get; set; }
    }
}
