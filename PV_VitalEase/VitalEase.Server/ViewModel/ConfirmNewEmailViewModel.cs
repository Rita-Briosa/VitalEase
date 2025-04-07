namespace VitalEase.Server.ViewModel
{
    /// <summary>
    /// Represents the view model used for confirming a new email address.
    /// </summary>
    /// <remarks>
    /// This view model contains a token that is used to validate and confirm the new email address.
    /// </remarks>
    public class ConfirmNewEmailViewModel
    {
        /// <summary>
        /// Gets or sets the confirmation token for the new email address.
        /// </summary>
        public string Token { get; set; }
    }
}
