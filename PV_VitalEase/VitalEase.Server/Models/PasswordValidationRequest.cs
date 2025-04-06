namespace VitalEase.Server.Models
{
    /// <summary>
    /// Represents a request for password validation containing the user's email and password.
    /// </summary>
    /// <remarks>
    /// This class is used to encapsulate the necessary data to verify whether the provided password is valid for the user.
    /// </remarks>
    public class PasswordValidationRequest
    {
        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the password to be validated.
        /// </summary>
        public string Password { get; set; }
    }
}
