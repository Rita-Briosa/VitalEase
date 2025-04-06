using System.ComponentModel.DataAnnotations;

namespace VitalEase.Server.ViewModel
{
    public class DeleteAccountConfirmationViewModel
    {
        [Required(ErrorMessage = "The email is required")]
        [EmailAddress(ErrorMessage = "Email is invalid")]
        public string Email { get; set; }
        public string Token { get; set; }
    }
}
