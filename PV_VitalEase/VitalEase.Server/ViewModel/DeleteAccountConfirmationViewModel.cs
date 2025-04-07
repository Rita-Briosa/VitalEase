using System.ComponentModel.DataAnnotations;

namespace VitalEase.Server.ViewModel
{
    /// <summary>
    /// Represents the view model used for confirming an account deletion.
    /// </summary>
    /// <remarks>
    /// This view model contains the user's email address and a token required for confirming the account deletion.
    /// </remarks>
    public class DeleteAccountConfirmationViewModel
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
        /// Gets or sets the confirmation token for account deletion.
        /// </summary>
        /// <remarks>
        /// This token is used to confirm the deletion of the account.
        /// </remarks>
        public string Token { get; set; }
    }
}
