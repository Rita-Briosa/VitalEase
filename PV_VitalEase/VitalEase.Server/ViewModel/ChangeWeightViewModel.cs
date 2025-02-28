using System.ComponentModel.DataAnnotations;

namespace VitalEase.Server.ViewModel
{
    public class ChangeWeightViewModel
    {
        [Required(ErrorMessage = "The email is required")]
        [EmailAddress(ErrorMessage = "Email is invalid")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Weight is required")]
        [Range(30, 450, ErrorMessage = "Weight must be between 30 and 250 kg.")]
        public int Weight { get; set; }
    }
}
