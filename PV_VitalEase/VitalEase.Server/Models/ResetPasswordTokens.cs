namespace VitalEase.Server.Models
{
    public class ResetPasswordTokens
    {
        public int Id { get; set; }
        public string TokenId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
      
 
    }
}
