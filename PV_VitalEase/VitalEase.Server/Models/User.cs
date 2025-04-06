
namespace VitalEase.Server.Models
{
    /// <summary>
    /// Represents a user of the system, including authentication details, profile information, and associated routines.
    /// </summary>
    /// <remarks>
    /// This class encapsulates user credentials, account status, and various properties that support session management and user administration.
    /// It also provides methods for login, registration, password reset, account deletion, and checking for administrative privileges.
    /// </remarks>
    public class User
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the hashed password of the user.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the type of the user (e.g., Standard, Admin).
        /// </summary>
        public UserType Type { get; set; }

        /// <summary>
        /// Gets or sets the profile associated with the user.
        /// </summary>
        public Profile Profile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user's email has been verified.
        /// </summary>
        public bool? IsEmailVerified { get; set; }

        /// <summary>
        /// Gets or sets the session token for the user.
        /// </summary>
        public string? SessionToken { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the user's password was last changed.
        /// </summary>
        public DateTime? PasswordLastChanged { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the current session token was created.
        /// </summary>
        public DateTime? SessionTokenCreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the collection of routines associated with the user.
        /// </summary>
        public List<Routine> Routines { get; set; } = new();
    }
}