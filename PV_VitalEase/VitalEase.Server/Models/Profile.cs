namespace VitalEase.Server.Models
{
    /// <summary>
    /// Represents a user's profile containing personal details and health information.
    /// </summary>
    public class Profile
    {
        /// <summary>
        /// Gets or sets the unique identifier for the profile.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the username associated with the profile.
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
        /// Gets or sets a value indicating whether the user has heart problems.
        /// </summary>
        public bool? HasHeartProblems { get; set; }

        /// <summary>
        /// Calculates the Body Mass Index (BMI) for the user based on their weight and height.
        /// </summary>
        /// <returns>
        /// A <see cref="double"/> representing the calculated BMI.
        /// </returns>
        public double CalculateBMI()
        {
            return Weight / (Height * Height);
        }

        /// <summary>
        /// Retrieves a summary of the profile including the username, gender, and BMI.
        /// </summary>
        /// <returns>
        /// A string containing the username, gender, and the calculated BMI formatted to two decimal places.
        /// </returns>
        public string GetProfileSummary()
        {
            return $"Username: {Username}, Gender: {Gender}, BMI: {CalculateBMI():F2}";
        }
    }
}
