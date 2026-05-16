namespace FlowDesk.Application.Features.Users.DTOs
{
    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = null!;
        public string? ManagerName { get; set; } = null;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
