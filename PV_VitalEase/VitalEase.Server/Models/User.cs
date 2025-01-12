
namespace VitalEase.Server.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public UserType Type { get; set; }
        public Profile Profile { get; set; }

        public bool Login(string email, string password)
        {
            // Implementation here
            return true;
        }

        public bool Register(RegisterRequest request)
        {
            // Implementation here
            return true;
        }

        public bool ResetPassword(string email)
        {
            // Implementation here
            return true;
        }

        public bool DeleteAccount()
        {
            // Implementation here
            return true;
        }

        public bool IsAdmin()
        {
            return Type == UserType.Admin;
        }
    }
}