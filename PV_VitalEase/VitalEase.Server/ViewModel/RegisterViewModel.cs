using System.ComponentModel.DataAnnotations;

namespace VitalEase.Server.ViewModel
{
    /// <summary>
    /// Represents the view model used for user registration.
    /// </summary>
    /// <remarks>
    /// This view model contains properties required for creating a new user account. It includes the user's username, birth date, email, height,
    /// weight, gender, password, and a flag indicating if the user has heart problems. Each property is decorated with data annotations to ensure
    /// that the input meets the necessary validation requirements.
    /// </remarks>
    public class RegisterViewModel
    {
        /// <summary>
        /// Gets or sets the username of the new user.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The username is required")]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the birth date of the new user.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The birth date is required")]
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// Gets or sets the email address of the new user.
        /// </summary>
        /// <remarks>
        /// This field is required and must be a valid email address.
        /// </remarks>
        [Required(ErrorMessage = "The email is required")]
        [EmailAddress(ErrorMessage = "Email is invalid")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the height of the new user in centimeters.
        /// </summary>
        /// <remarks>
        /// This field is required and must be between 90 and 251 cm.
        /// </remarks>
        [Range(90, 251, ErrorMessage = "Height must be between 90 and 251 cm.")]
        [Required(ErrorMessage = "Height is required")]
        public  int Height{ get; set; }

        /// <summary>
        /// Gets or sets the weight of the new user in kilograms.
        /// </summary>
        /// <remarks>
        /// This field is required and must be between 30 and 400 kg.
        /// </remarks>
        [Range(30, 400, ErrorMessage = "Weight must be between 30 and 400 kg.")]
        [Required(ErrorMessage = "Weight is required")]
        public int Weight { get; set; }

        /// <summary>
        /// Gets or sets the gender of the new user.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }

        /// <summary>
        /// Gets or sets the password of the new user.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The password is required")]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has heart problems.
        /// </summary>
        /// <remarks>
        /// This field is optional.
        /// </remarks>
        public bool HeartProblems{ get; set; }


    }
}
