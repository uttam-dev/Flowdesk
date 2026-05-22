using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Common.Interfaces;
using FlowDesk.Application.Features.Users.Commands;
using FlowDesk.Application.Features.Users.DTOs;
using FlowDesk.Application.Features.Users.Queries;
using FlowDesk.Domain.DTOs;
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

        //Get manager's
        [Authorize("RequireAdminRole")]
        [HttpGet("managers")]
        public async Task<IActionResult> GetManagers()
        {
            var managers = await _mediator.Send(new GetManagersQuery());
            return Ok(new ApiResponseDto() { Message = "Managers fetched successfully.", Data = managers });
        }

        //Get support
        [Authorize("RequireAdminRole")]
        [HttpGet("support")]
        public async Task<IActionResult> GetSupportUsers()
        {
            var supportUsers = await _mediator.Send(new GetSupportUserQuery());
            return Ok(new ApiResponseDto() { Message = "Support users fetched successfully.", Data = supportUsers });
        }


        //Get all roles
        [Authorize("RequireAdminRole")]
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _mediator.Send(new GetRolesQuery());
            return Ok(new ApiResponseDto() { Message = "Roles fetched successfully.", Data = roles });
        }


        [Authorize(policy: "RequireAdminRole")]
        //Create new user
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto userReq)
        {
            var user = await _mediator.Send(new CreateUserCommand(userReq));
            return Ok(new ApiResponseDto() { Message = "User created successfully.", Data = user });
        }

        // Create Bulk user 
        [HttpPost("bulk-upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> BulkUpload([FromForm] BulkUploadRequest Dto, [FromServices] IFileParser parser)
        {
            if (Dto.File == null || Dto.File.Length == 0)
                throw new BadRequestException("Invalid file");

            var ext = Path.GetExtension(Dto.File.FileName).ToLower();

            if (ext != ".csv" && ext != ".xlsx")
                throw new BadRequestException("Only CSV or Excel allowed");

            using var stream = Dto.File.OpenReadStream();

            // Convert here
            var users = await parser.ParseAsync(stream, Dto.File.FileName);

            var result = await _mediator.Send(new BulkCreateUsersCommand
            {
                Users = users
            });

            return Ok(new ApiResponseDto
            {
                Message = "Bulk user import completed successfully.",
                Data = result
            });
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
            var currentUserId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _mediator.Send(new SoftDeleteUserCommand(id, currentUserId));
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

        [Authorize("RequireAdminRole")]
        //active user
        [HttpPatch("{id}/active")]
        public async Task<IActionResult> ActiveUser(int id)
        {
            var currentUserId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _mediator.Send(new ActiveUserCommand(id, currentUserId));
            return Ok(new ApiResponseDto() { Message = "User activated successfully." });
        }

        [Authorize("RequireAdminRole")]
        //active user
        [HttpPatch("{id}/deactive")]
        public async Task<IActionResult> DeactiveUser(int id)
        {
            var currentUserId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _mediator.Send(new DeactiveUserCommand(id, currentUserId));
            return Ok(new ApiResponseDto() { Message = "User activated successfully." });
        }

    }

    public class BulkUploadRequest
    {
        public IFormFile? File { get; set; }
    }
}