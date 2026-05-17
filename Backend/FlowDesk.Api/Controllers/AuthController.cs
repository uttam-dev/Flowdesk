using FlowDesk.Application.Features.Users.Queries;
using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowDesk.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService _authService,IMediator _mediator) : ControllerBase
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
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto refreshToken)
        {
            var result = await _authService.RefreshTokenAsync(refreshToken);
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
    }
}
