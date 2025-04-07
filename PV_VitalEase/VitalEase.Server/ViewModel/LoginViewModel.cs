using System.ComponentModel.DataAnnotations;

namespace VitalEase.Server.ViewModel
{
    /// <summary>
    /// Represents the view model used for logging in a user.
    /// </summary>
    /// <remarks>
    /// This view model contains the user's email address and password, which are required for authentication.
    /// It also includes a boolean flag indicating whether the user has chosen to be remembered on the device.
    /// The email must be in a valid email format.
    /// </remarks>
    public class LoginViewModel
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
        /// Gets or sets the user's password.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The password is required")]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user wants to be remembered on the device.
        /// </summary>
        public bool RememberMe { get; set; }
    }
}