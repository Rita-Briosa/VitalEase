using System.ComponentModel.DataAnnotations;

namespace VitalEase.Server.ViewModel
{
    public class ChangeUsernameViewModel
    {
        [Required(ErrorMessage = "The username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "The email is required")]
        [EmailAddress(ErrorMessage = "Email is invalid")]
        public string Email { get; set; }
    }
}
