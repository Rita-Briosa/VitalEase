namespace VitalEase.Server.ViewModel
{
    using System.ComponentModel.DataAnnotations;
    public class ChangeEmailViewModel
    {
        [Required(ErrorMessage = "The email is required")]
        [EmailAddress(ErrorMessage = "Email is invalid")]
        public string Email { get; set; }

        [Required(ErrorMessage = "The old password is required")]
        public string Password { get; set; }

        [Required(ErrorMessage = "The new email is required")]
        [EmailAddress(ErrorMessage = "New Email is invalid")]
        public string NewEmail { get; set; }
    }
}
