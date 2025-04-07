using System.ComponentModel.DataAnnotations;

namespace VitalEase.Server.ViewModel
{
    /// <summary>
    /// Represents the view model used for updating a user's gender.
    /// </summary>
    /// <remarks>
    /// This view model contains the user's email address and the new gender value. Both fields are required.
    /// The email must be valid, and the gender must be either "Male" or "Female" (case-insensitive).
    /// </remarks>
    public class ChangeGenderViewModel
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
        /// Gets or sets the new gender of the user.
        /// </summary>
        /// <remarks>
        /// This field is required and must match either "Male" or "Female" (case-insensitive).
        /// </remarks>
        [Required(ErrorMessage = "Gender is required")]
        [RegularExpression("^(Male|Female|male|female)$", ErrorMessage = "Gender must be 'Male' or 'Female'")]
        public string Gender { get; set; }

    }
}
