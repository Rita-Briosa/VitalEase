using System.ComponentModel.DataAnnotations;

namespace VitalEase.Server.ViewModel
{
    public class ConfirmOldEmailViewModel
    {
        public string Token { get; set; }

        [Required(ErrorMessage = "The New email is required")]
        [EmailAddress(ErrorMessage = "New Email is invalid")]
        public string NewEmail { get; set; }
    }
}
