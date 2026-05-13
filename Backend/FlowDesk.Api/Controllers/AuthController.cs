using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace FlowDesk.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService _authService) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);
            return Ok(
                new ApiResponseDto()
                {
                    StatusCode = 200,
                    Message = "Login successful.",
                    Data = result
                });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string token)
        {
            var result = await _authService.RefreshTokenAsync(token);
            return Ok(
                new ApiResponseDto()
                {
                    StatusCode = 200,
                    Message = "Token refreshed.",
                    Data = result
                });
        }
    }
}
