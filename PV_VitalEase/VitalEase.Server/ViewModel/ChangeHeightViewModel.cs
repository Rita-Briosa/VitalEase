using System.ComponentModel.DataAnnotations;

namespace VitalEase.Server.ViewModel
{
    public class ChangeHeightViewModel
    {
        [Required(ErrorMessage = "The email is required")]
        [EmailAddress(ErrorMessage = "Email is invalid")]
        public string Email { get; set; }

        [Range(125, 250, ErrorMessage = "Height must be between 125 and 250 cm.")]
        [Required(ErrorMessage = "Height is required")]
        public int Height { get; set; }
    }
}
