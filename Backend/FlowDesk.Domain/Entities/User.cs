namespace FlowDesk.Domain.Entities
{

    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public int? ManagerId { get; set; }
        public User? Manager { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }

        public ICollection<User>? Subordinates { get; set; }
        public ICollection<Request>? CreatedRequests { get; set; }
        public ICollection<Request>? AssignedRequests { get; set; }
        public ICollection<Comment>? Comments { get; set; }
        public ICollection<RequestHistory>? RequestHistories { get; set; }
        public ICollection<RefreshToken>? RefreshTokens { get; set; }
    }
}
