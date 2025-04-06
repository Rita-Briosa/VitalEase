namespace VitalEase.Server.Models
{
    /// <summary>
    /// Represents a request to register a new user.
    /// </summary>
    /// <remarks>
    /// This class encapsulates all the necessary information required for registering a new user,
    /// including email, password, username, personal details such as gender, weight, height, birthdate,
    /// and whether the user has a history of cardiac issues.
    /// </remarks>
    public class RegisterRequest
    {
        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the password for the new user.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the desired username for the new user.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the gender of the user.
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// Gets or sets the weight of the user in kilograms.
        /// </summary>
        public double Weight { get; set; }

        /// <summary>
        /// Gets or sets the height of the user in meters.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Gets or sets the birthdate of the user.
        /// </summary>
        public DateTime Birthdate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has a history of cardiac issues.
        /// </summary>
        public bool IsCardiac { get; set; }
    }
}