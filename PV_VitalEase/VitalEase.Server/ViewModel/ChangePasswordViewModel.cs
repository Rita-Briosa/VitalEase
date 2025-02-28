namespace VitalEase.Server.ViewModel
{
    using System.ComponentModel.DataAnnotations;
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "The email is required")]
        [EmailAddress(ErrorMessage = "Email is invalid")]
        public string Email { get; set; }

        [Required(ErrorMessage = "The old password is required")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "The new password is required")]
        public string NewPassword { get; set; }
    }
}
