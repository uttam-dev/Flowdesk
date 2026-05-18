using FlowDesk.Domain.Interfaces;
using FlowDesk.Domain.DTOs;
using Microsoft.Extensions.Logging;
using FlowDesk.Application.Common.Exceptions;

namespace FlowDesk.Application.Services
{
    public class AuthService(
        IUserRepository _userRepo,
        IJwtTokenService _jwtService,
        IRefreshTokenService _refreshTokenService,
        ILogger<AuthService> _logger) : IAuthService
    {
        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            _logger.LogInformation("Starting {Operation}", nameof(LoginAsync));

            var user = await _userRepo.GetByEmailAsync(request.Email);

            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found for Email {Email}", request.Email);
                throw new UnauthorizedException("Invalid credentials");
            }

            if (user.IsDeleted || !user.IsActive)
            {
                _logger.LogWarning("Login failed: Inactive/Deleted user {UserId}", user.UserId);
                throw new UnauthorizedException("Invalid credentials");
            }

            if (!PasswordService.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: Invalid password for UserId {UserId}", user.UserId);
                throw new UnauthorizedException("Invalid credentials");
            }

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            await _refreshTokenService.SaveRefreshToken(user.UserId, refreshToken);

            _logger.LogInformation("Login successful for UserId {UserId}", user.UserId);

            return new AuthResponseDto
            {
                FullName = user.FullName,
                Email = user.Email,
                RoleName = user.Role.RoleName,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto Dto)
        {
            _logger.LogInformation("Starting {Operation}", nameof(RefreshTokenAsync));

            var token = await _refreshTokenService.GetToken(Dto.RefreshToekn!);

            if (token == null)
            {
                _logger.LogWarning("Refresh token not found");
                throw new BadRequestException("Invalid refresh token");
            }

            if (token.ExpiryDate < DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token expired for UserId {UserId}", token.UserId);
                throw new BadRequestException("Invalid refresh token");
            }

            var user = await _userRepo.GetByIdAsync(token.UserId);

            if (user == null)
            {
                _logger.LogWarning("User not found for RefreshToken UserId {UserId}", token.UserId);
                throw new BadRequestException("User not found");
            }

            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            await _refreshTokenService.SaveRefreshToken(user.UserId, newRefreshToken);

            _logger.LogInformation("Refresh token rotated for UserId {UserId}", user.UserId);

            return new AuthResponseDto
            {
                FullName = user.FullName,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }
    }
}
