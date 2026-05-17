namespace FlowDesk.Domain.DTOs
{
    public class AuthResponseDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? RoleName { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
