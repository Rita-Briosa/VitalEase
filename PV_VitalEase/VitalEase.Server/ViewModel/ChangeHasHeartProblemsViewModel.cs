using System.ComponentModel.DataAnnotations;

namespace VitalEase.Server.ViewModel
{
    public class ChangeHasHeartProblemsViewModel
    {
        [Required(ErrorMessage = "The email is required")]
        [EmailAddress(ErrorMessage = "Email is invalid")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Has heart problems is required")]
        public bool HasHeartProblems { get; set; }
    }
}
