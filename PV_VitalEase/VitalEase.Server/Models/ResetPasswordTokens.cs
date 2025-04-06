namespace VitalEase.Server.Models
{
    /// <summary>
    /// Represents a token used for resetting a user's password.
    /// </summary>
    /// <remarks>
    /// This class encapsulates the details related to password reset tokens, including a unique token identifier,
    /// the timestamps for when the token was created and when it expires, and a flag indicating whether the token has been used.
    /// </remarks>
    public class ResetPasswordTokens
    {
        /// <summary>
        /// Gets or sets the unique identifier for the password reset token record.
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
        /// </summary>
        public bool IsUsed { get; set; }
      
 
    }
}
