
namespace VitalEase.Server.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public UserType Type { get; set; }
        public Profile Profile { get; set; }

        public bool? IsEmailVerified { get; set; }

        // Custom property to track session token
        public string? SessionToken { get; set; }

        // Property to track when the password was last changed
        public DateTime? PasswordLastChanged { get; set; }

        // Property to track when the session token was created (optional)
        public DateTime? SessionTokenCreatedAt { get; set; }

        public List<Routine> Routines { get; set; } = new();


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