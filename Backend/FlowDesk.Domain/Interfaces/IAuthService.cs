using FlowDesk.Domain.DTOs;

namespace FlowDesk.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task<RefreshTokenResponseDto> RefreshTokenAsync(string? refreshToken);
        Task LogoutAsync(string? refreshToken);
    }
}
