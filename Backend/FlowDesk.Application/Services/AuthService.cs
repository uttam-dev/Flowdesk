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
            _logger.LogInformation("Login attempt started for Email: {Email}", request.Email);

            var user = await _userRepo.GetByEmailAsync(request.Email);

            if (user == null)
            {
                throw new UnauthorizedException("Invalid credentials");
            }

            if (user.IsDeleted || !user.IsActive)
            {
                throw new UnauthorizedException("Invalid credentials");
            }

            if (!PasswordService.VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedException("Invalid credentials");
            }

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            await _refreshTokenService.SaveRefreshToken(user.UserId, refreshToken);

            _logger.LogInformation("Login successful for UserId: {UserId}", user.UserId);

            return new AuthResponseDto
            {
                Name = user.FullName,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            _logger.LogInformation("Refresh token request received");

            var token = await _refreshTokenService.GetToken(refreshToken);

            if (token == null || token.ExpiryDate < DateTime.UtcNow)
            {
                throw new BadRequestException("Invalid refresh token");
            }

            var user = await _userRepo.GetByIdAsync(token.UserId);

            if (user == null)
            {
                throw new BadRequestException("User not found");
            }

            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            token.IsRevoked = true;
            await _refreshTokenService.SaveRefreshToken(user.UserId, newRefreshToken);

            _logger.LogInformation("Refresh token rotated for UserId: {UserId}", user.UserId);

            return new AuthResponseDto
            {
                Name = user.FullName,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }
    }
}
