using System.ComponentModel.DataAnnotations;

namespace VitalEase.Server.ViewModel
{
    public class ResetPasswordViewModel
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
        
    }
}
