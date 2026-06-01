using FlowDesk.Application.Features.Chat.Commands;
using FlowDesk.Application.Features.Chat.DTOs;
using FlowDesk.Domain.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowDesk.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController(IMediator _mediator) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] ChatRequestDto request)
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value!;

            var result = await _mediator.Send(new AskChatbotCommand(request.Message, userId, userRole));

            return Ok(new ApiResponseDto
            {
                Message = "Success",
                Data = result
            });
        }
    }
}
