using System.ComponentModel.DataAnnotations;

namespace VitalEase.Server.ViewModel
{
    /// <summary>
    /// Represents the view model used for initiating a password recovery process.
    /// </summary>
    /// <remarks>
    /// This view model contains the user's email address, which is required for sending a password reset link.
    /// The email must be in a valid format.
    /// </remarks>
    public class ForgotPasswordViewModel
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
    }
}
