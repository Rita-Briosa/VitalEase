namespace VitalEase.Server.ViewModel
{
    /// <summary>
    /// Represents the view model used for verifying a user's email address.
    /// </summary>
    /// <remarks>
    /// This view model contains a token that is used to validate and confirm the user's email address.
    /// </remarks>
    public class VerifyEmailViewModel
    {
        /// <summary>
        /// Gets or sets the verification token for the email address.
        /// </summary>
        public string Token { get; set; }
    }
}
