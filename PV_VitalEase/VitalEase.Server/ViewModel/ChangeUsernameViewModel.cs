using System.ComponentModel.DataAnnotations;

namespace VitalEase.Server.ViewModel
{
    /// <summary>
    /// Represents the view model used for updating a user's username.
    /// </summary>
    /// <remarks>
    /// This view model contains the new username and the user's email address. Both properties are required,
    /// with the email property being validated to ensure it is in a correct email format.
    /// </remarks>
    public class ChangeUsernameViewModel
    {
        /// <summary>
        /// Gets or sets the new username.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The username is required")]
        public string Username { get; set; }

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
