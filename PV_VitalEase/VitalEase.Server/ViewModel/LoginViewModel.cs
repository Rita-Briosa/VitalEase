using System.ComponentModel.DataAnnotations;

namespace VitalEase.Server.ViewModel
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "The email is required")]
        [EmailAddress(ErrorMessage = "Email is invalid")]
        public string Email { get; set; }

        [Required(ErrorMessage = "The password is required")]
        public string Password { get; set; }
    }
}