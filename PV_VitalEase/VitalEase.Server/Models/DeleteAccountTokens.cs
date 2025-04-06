namespace VitalEase.Server.Models
{
    public class DeleteAccountTokens
    {
        public int Id { get; set; }
        public string TokenId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
    }
}
