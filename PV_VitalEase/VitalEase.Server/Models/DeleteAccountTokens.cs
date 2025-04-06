namespace VitalEase.Server.Models
{
    /// <summary>
    /// Represents a token used for account deletion operations.
    /// </summary>
    /// <remarks>
    /// This class encapsulates details related to account deletion tokens, including a unique token identifier,
    /// creation time, expiration time, and usage status.
    /// </remarks>
    public class DeleteAccountTokens
    {
        /// <summary>
        /// Gets or sets the unique identifier for the token record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique token identifier as a string.
        /// </summary>
        public string TokenId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the token was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the token expires.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the token has been used.
        /// <c>true</c> if the token has been used; otherwise, <c>false</c>.
        /// </summary>
        public bool IsUsed { get; set; }
    }
}
