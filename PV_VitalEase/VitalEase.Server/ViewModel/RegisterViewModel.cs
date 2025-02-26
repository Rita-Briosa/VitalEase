using System.ComponentModel.DataAnnotations;

namespace VitalEase.Server.ViewModel
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "The username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "The birth date is required")]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "The email is required")]
        [EmailAddress(ErrorMessage = "Email is invalid")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Height is required")]
        public  int Height{ get; set; }

        [Required(ErrorMessage = "Weight is required")]
        public int Weight { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [RegularExpression("^(Male|Female)$", ErrorMessage = "Gender must be 'Male' or 'Female'")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "The password is required")]
        public string Password { get; set; }

        public bool HeartProblems{ get; set; }


    }
}
