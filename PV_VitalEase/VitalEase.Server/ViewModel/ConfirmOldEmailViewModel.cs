using System.ComponentModel.DataAnnotations;

namespace VitalEase.Server.ViewModel
{
    /// <summary>
    /// Represents the view model used for confirming an old email address during the process of updating to a new email address.
    /// </summary>
    /// <remarks>
    /// This view model contains a token for validation and the new email address to be confirmed.
    /// The new email address is required and must be in a valid email format.
    /// </remarks>
    public class ConfirmOldEmailViewModel
    {
        /// <summary>
        /// Gets or sets the confirmation token for the old email address.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the new email address to be confirmed.
        /// </summary>
        /// <remarks>
        /// This field is required and must be a valid email address.
        /// </remarks>
        [Required(ErrorMessage = "The New email is required")]
        [EmailAddress(ErrorMessage = "New Email is invalid")]
        public string NewEmail { get; set; }
    }
}
