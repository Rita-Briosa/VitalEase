using VitalEase.Server.Models;

public class User
{
    public string Email { get; set; }
    public string Password { get; set; }
    public UserType Type { get; set; }

    public bool Login(string email, string password)
    {
        // Logic for login
        return true;
    }

    public bool Register(RegisterRequest data)
    {
        // Logic for registration
        return true;
    }

    public void ResetPassword(string email)
    {
        // Logic for resetting password
    }

    public void DeleteAccount()
    {
        // Logic for deleting account
    }

    public bool IsAdmin()
    {
        return Type == UserType.Admin;
    }
}
