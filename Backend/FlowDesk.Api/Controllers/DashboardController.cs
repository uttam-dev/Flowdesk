using FlowDesk.Application.Features.Dashboard.Queries;
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
    public class DashboardController(IMediator mediator) : ControllerBase
    {
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var roleString = User.FindFirst(ClaimTypes.Role)!.Value;
            var role = Enum.Parse<RoleEnum>(roleString, ignoreCase: true);
            var data = await mediator.Send(new GetDashboardQuery(userId, role));
            return Ok(new ApiResponseDto
            {
                Message = "Dashboard fetched successfully",
                Data = data
            });
        }

        [Authorize(Roles = "Admin,Support")]
        [HttpGet("sla-summary")]
        public async Task<IActionResult> GetSlaSummary()
        {
            var data = await mediator.Send(new GetSlaSummaryQuery());
            return Ok(new ApiResponseDto
            {
                Message = "SLA summary fetched successfully",
                Data = data
            });
        }
    }
}
