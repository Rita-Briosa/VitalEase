namespace VitalEase.Server.ViewModel
{
    /// <summary>
    /// Represents the view model used for handling an account deletion token.
    /// </summary>
    /// <remarks>
    /// This view model contains a token that is used during the account deletion process.
    /// </remarks>
    public class DeleteAccountTokenViewModel
    {
        /// <summary>
        /// Gets or sets the account deletion token.
        /// </summary>
        public string Token { get; set; }
    }
}
