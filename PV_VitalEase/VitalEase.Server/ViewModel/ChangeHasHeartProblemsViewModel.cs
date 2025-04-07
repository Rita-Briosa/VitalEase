using System.ComponentModel.DataAnnotations;

namespace VitalEase.Server.ViewModel
{
    /// <summary>
    /// Represents the view model used for updating whether a user has heart problems.
    /// </summary>
    /// <remarks>
    /// This view model contains the user's email address and a boolean indicating if the user has heart problems.
    /// Both fields are required, with the email validated to ensure it is in a proper email format.
    /// </remarks>
    public class ChangeHasHeartProblemsViewModel
    {
        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        /// <remarks>
        /// This field is required and must be in a valid email format.
        /// </remarks>
        [Required(ErrorMessage = "The email is required")]
        [EmailAddress(ErrorMessage = "Email is invalid")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has heart problems.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "Has heart problems is required")]
        public bool HasHeartProblems { get; set; }
    }
}
