public class Profile
{
    public string Username { get; set; }
    public string Gender { get; set; }
    public double Weight { get; set; }
    public double Height { get; set; }
    public DateTime Birthdate { get; set; }
    public bool IsCardiac { get; set; }

    public double CalculateBMI()
    {
        return Weight / (Height * Height);
    }

    public string GetProfileSummary()
    {
        // Logic for profile summary
        return "Profile Summary";
    }
}
