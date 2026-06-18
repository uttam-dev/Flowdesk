using FlowDesk.Application.Features.Users.Queries;
using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;

namespace FlowDesk.Api.Controllers
{
    [EnableCors("AllowAgent")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService _authService, IMediator _mediator, IConfiguration configuration) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);
            Response.Cookies.Append(configuration["CookieOptions:AccessTokenName"]!, result.AccessToken!, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddMinutes(int.Parse(configuration["Jwt:ExpireMinutes"]!))
            });

            Response.Cookies.Append(configuration["CookieOptions:RefreshTokenName"]!, result.RefreshToken!, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                //Path = "/auth/refresh",
                Expires = DateTimeOffset.UtcNow.AddDays(int.Parse(configuration["RefreshToken:ExpireDays"]!))
            });

            return Ok(
                new ApiResponseDto()
                {
                    StatusCode = 200,
                    Message = "Login successful.",
                    Data = result
                });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies[configuration["CookieOptions:RefreshTokenName"]!]
                    ?? Request.Headers["X-Refresh-Token"].ToString();

            Log.Information(refreshToken);
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized();

            var result = await _authService.RefreshTokenAsync(refreshToken);

            // Update cookies for browser
            Response.Cookies.Append(configuration["CookieOptions:AccessTokenName"]!, result.AccessToken!, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddMinutes(int.Parse(configuration["Jwt:ExpireMinutes"]!))
            });
            Response.Cookies.Append(configuration["CookieOptions:RefreshTokenName"]!, result.RefreshToken!, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                //Path = "/auth/refresh",
                Expires = DateTimeOffset.UtcNow.AddDays(int.Parse(configuration["RefreshToken:ExpireDays"]!))
            });

            return Ok(
                new ApiResponseDto()
                {
                    StatusCode = 200,
                    Message = "Token refreshed.",
                    Data = result
                });
        }
        //Get current user details
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUserDetails()
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var user = await _mediator.Send(new GetUserByIdQuery(userId));
            return Ok(new ApiResponseDto() { Message = "Current user details fetched successfully.", Data = user });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Read refresh token from cookie or header (hybrid)
            var refreshToken = Request.Cookies[configuration["CookieOptions:RefreshTokenName"]!]
                            ?? Request.Headers["X-Refresh-Token"].ToString();


            await _authService.LogoutAsync(refreshToken);
            // Clear both cookies from browser
            Response.Cookies.Delete(configuration["CookieOptions:AccessTokenName"]!, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });

            Response.Cookies.Delete(configuration["CookieOptions:RefreshTokenName"]!, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                //Path = "/auth/refresh"  
            });

            return Ok(new ApiResponseDto { Message = "Logged out successfully" });
        }

        [Authorize]
        //Reset user password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] UserResetSelfPasswordDto resetPasswordDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            await _authService.ResetSelfPassword(userId, resetPasswordDto);
            return Ok(new ApiResponseDto() { Message = "Password reset successfully." });

        }
    }
}
