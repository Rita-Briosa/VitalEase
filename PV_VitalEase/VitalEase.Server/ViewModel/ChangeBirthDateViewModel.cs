using System.ComponentModel.DataAnnotations;

namespace VitalEase.Server.ViewModel
{
    /// <summary>
    /// Represents the view model used for updating a user's birth date.
    /// </summary>
    /// <remarks>
    /// This view model contains the user's email and the new birth date. Both properties are mandatory,
    /// with the email property being validated to ensure it is in a proper email format.
    /// </remarks>
    public class ChangeBirthDateViewModel
    {
        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        /// <remarks>
        /// This field is required and must be a valid email address.
        /// </remarks>
        [Required(ErrorMessage = "The email is required")]
        [EmailAddress(ErrorMessage = "Email is invalid")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the new birth date of the user.
        /// </summary>
        /// <remarks>
        /// This field is required.
        /// </remarks>
        [Required(ErrorMessage = "The birth date is required")]
        public DateTime BirthDate { get; set; }
    }
}
