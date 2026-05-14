using FlowDesk.Application.Features.Users.Commands;
using FlowDesk.Application.Features.Users.DTOs;
using FlowDesk.Application.Features.Users.Queries;
using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowDesk.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IMediator _mediator) : ControllerBase
    {

        //Get all users
        [Authorize(policy: "RequireAdminRole")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] FilterUserDataQueryDto query)
        {
            var data = await _mediator.Send(new GetAllUsersQuery(query));

            return Ok(
                new ApiResponseDto
                {
                    Message = "All Users fetched successfully.",
                    Data = data
                });
        }

        [Authorize(policy: "RequireAdminRole")]
        //Get user by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _mediator.Send(new GetUserByIdQuery(id));
            return Ok(new ApiResponseDto() { Message = "User fetched successfully.", Data = user });
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

        [Authorize(policy: "RequireAdminRole")]
        //Create new user
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto userReq)
        {
            var user = await _mediator.Send(new CreateUserCommand(userReq));
            return Ok(new ApiResponseDto() { Message = "User created successfully.", Data = user });
        }


        [Authorize(policy: "RequireAdminRole")]
        //Reset user password
        [HttpPatch("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] UserResetPasswordDto resetPasswordDto)
        {
            await _mediator.Send(new ResetPasswordCommand(id, resetPasswordDto));
            return Ok(new ApiResponseDto() { Message = "Password reset successfully." });

        }

        [Authorize(policy: "RequireAdminRole")]
        //Soft delete user
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteUser(int id)
        {
            await _mediator.Send(new SoftDeleteUserCommand(id));
            return Ok(new ApiResponseDto() { Message = "User deleted successfully." });
        }

        [Authorize(policy: "RequireAdminRole")]
        //Update user details
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDetailsDto updateUserDto)
        {
            var updatedUser = await _mediator.Send(new UpdateUserDetailsCommand(id, updateUserDto));
            return Ok(new ApiResponseDto() { Message = "User updated successfully.", Data = updatedUser });
        }
    }
}