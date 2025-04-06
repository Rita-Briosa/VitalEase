namespace VitalEase.Server.Models
{
    /// <summary>
    /// Represents a token used for resetting a user's email address.
    /// </summary>
    /// <remarks>
    /// This class encapsulates the properties related to email reset tokens, including a unique token identifier,
    /// creation and expiration timestamps, and flags indicating whether the token has been used overall and specifically
    /// for the old email address.
    /// </remarks>
    public class ResetEmailTokens
    {
        /// <summary>
        /// Gets or sets the unique identifier for the reset email token record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique token identifier used for email reset operations.
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
        /// </summary>
        public bool IsUsed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the token has been used specifically on the old email.
        /// </summary>
        public bool IsUsedOnOldEmail { get; set; }

    }
}
