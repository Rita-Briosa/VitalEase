using System.ComponentModel.DataAnnotations;

namespace VitalEase.Server.ViewModel
{
    public class ChangeBirthDateViewModel
    {
        [Required(ErrorMessage = "The email is required")]
        [EmailAddress(ErrorMessage = "Email is invalid")]
        public string Email { get; set; }

        [Required(ErrorMessage = "The birth date is required")]
        public DateTime BirthDate { get; set; }
    }
}
