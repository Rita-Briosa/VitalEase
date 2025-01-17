using System.ComponentModel.DataAnnotations;

namespace VitalEase.Server.ViewModel
{
    public class ResetPasswordViewModel
    {
        public class ResetPasswordModel
        {
            public string Email { get; set; }
            public string NewPassword { get; set; }
        }
    }
}
