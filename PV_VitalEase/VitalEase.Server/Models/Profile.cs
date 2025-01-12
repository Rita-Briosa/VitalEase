namespace VitalEase.Server.Models
{
    public class Profile
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Gender { get; set; }
        public double Weight { get; set; }
        public double Height { get; set; }
        public DateTime Birthdate { get; set; }

        public double CalculateBMI()
        {
            return Weight / (Height * Height);
        }

        public string GetProfileSummary()
        {
            return $"Username: {Username}, Gender: {Gender}, BMI: {CalculateBMI():F2}";
        }
    }
}
