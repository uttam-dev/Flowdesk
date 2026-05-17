using FlowDesk.Domain.DTOs;

namespace FlowDesk.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshToken);
    }
}
