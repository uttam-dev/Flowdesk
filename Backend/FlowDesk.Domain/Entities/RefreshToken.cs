

namespace FlowDesk.Domain.Entities
{
    public class RefreshToken
    {
        public int RefreshTokenId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; } = false;

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
