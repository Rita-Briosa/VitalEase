using System.ComponentModel.DataAnnotations;

namespace VitalEase.Server.ViewModel
{
    /// <summary>
    /// Represents the view model used for resetting a user's password.
    /// </summary>
    /// <remarks>
    /// This view model contains a token for validating the password reset request and the new password to be set for the user.
    /// </remarks>
    public class ResetPasswordViewModel
    {
        /// <summary>
        /// Gets or sets the token used for resetting the password.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the new password for the user.
        /// </summary>
        public string NewPassword { get; set; }
        
    }
}
