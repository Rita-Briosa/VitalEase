using System.ComponentModel.DataAnnotations;

namespace VitalEase.Server.ViewModel
{
    /// <summary>
    /// Represents the view model used for requesting account deletion.
    /// </summary>
    /// <remarks>
    /// This view model contains the user's email address, which is required and must be a valid email.
    /// </remarks>
    public class DeleteAccountRequestViewModel
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
    }
}
