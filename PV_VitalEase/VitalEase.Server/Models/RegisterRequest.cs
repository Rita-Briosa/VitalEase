namespace VitalEase.Server.Models
{
    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public string Gender { get; set; }
        public double Weight { get; set; }
        public double Height { get; set; }
        public DateTime Birthdate { get; set; }
        public bool IsCardiac { get; set; }

        public bool Validate()
        {
            // Implementation here
            return true;
        }
    }
}