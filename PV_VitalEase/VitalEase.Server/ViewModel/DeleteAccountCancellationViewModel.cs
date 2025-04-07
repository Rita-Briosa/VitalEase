namespace VitalEase.Server.ViewModel
{
    /// <summary>
    /// Represents the view model used for cancelling an account deletion.
    /// </summary>
    /// <remarks>
    /// This view model contains a token that is used to cancel the deletion of an account.
    /// </remarks>
    public class DeleteAccountCancellationViewModel
    {
        /// <summary>
        /// Gets or sets the cancellation token.
        /// </summary>
        public string Token { get; set; }
    }
}
