namespace FlowDesk.Domain.DTOs
{
    public class AuthResponseDto
    {
        public string? Name { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
