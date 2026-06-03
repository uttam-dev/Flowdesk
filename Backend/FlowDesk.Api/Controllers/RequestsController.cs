using FlowDesk.Application.Features.Requests.Commands;
using FlowDesk.Application.Features.Requests.DTOs;
using FlowDesk.Domain.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FlowDesk.Application.Features.Requests.Queries;

namespace FlowDesk.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestsController(IMediator _mediator) : ControllerBase
    {
        // POST: api/requests
        [Authorize(policy: "CanCreateRequest")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateRequestDto dto)
        {
            var currentRole = User.FindFirst(ClaimTypes.Role)?.Value!;
            int userIdClaim = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var result = await _mediator.Send(new CreateRequestCommand(currentRole, userIdClaim, dto));

            return Ok(new ApiResponseDto
            {
                Message = "Request created successfully",
                Data = result
            });
        }


        // GET: api/requests
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] FilterRequestQueryDto query)
        {
            int currentUserId = Convert.ToInt16(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            string currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value!;
            var result = await _mediator.Send(new GetAllRequestsQuery(query, currentUserId, currentUserRole));

            return Ok(new ApiResponseDto
            {
                Message = "Requests fetched successfully",
                Data = result
            });
        }
        // GET: api/requests
        [Authorize("RequireManagerRole")]
        [HttpGet("team")]
        public async Task<IActionResult> GetTeamRequests([FromQuery] FilterRequestQueryDto query)
        {
            int currentUserId = Convert.ToInt16(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _mediator.Send(new GetAllTeamRequestsQuery(currentUserId, query));

            return Ok(new ApiResponseDto
            {
                Message = "Requests fetched successfully",
                Data = result
            });
        }

        // Get api/requests/remarks
        [Authorize]
        [HttpGet("remarks")]
        public async Task<IActionResult> GetRequestRemarks([FromQuery] FilterRequestRemarksQueryDto query)
        {
            int currentUserId = Convert.ToInt16(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _mediator.Send(new GetRequestRemarksQuery(query));
            return Ok(new ApiResponseDto
            {
                Message = "Request remarks fetched successfully",
                Data = result
            });
        }

        //// GET: api/requests/{id}
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetRequestByIdQuery(id));

            return Ok(new ApiResponseDto
            {
                Message = "Request fetched successfully",
                Data = result
            });
        }

        //// POST: api/requests/{id}/approve
        [Authorize("RequireManagerRole")]
        [HttpPatch("{reqestId}/approve")]
        public async Task<IActionResult> Approve(int reqestId, [FromBody] RemarkDto dto)
        {
            int userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _mediator.Send(new ApproveRequestCommand(reqestId, userId, dto));

            return Ok(new ApiResponseDto
            {
                Message = "Request approved successfully"
            });
        }

        //// POST: api/requests/{id}/reject
        [Authorize("RequireManagerRole")]
        [HttpPatch("{requestId}/reject")]
        public async Task<IActionResult> Reject(int requestId, RemarkDto dto)
        {
            int userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _mediator.Send(new RejectRequestCommand(requestId, userId, dto));

            return Ok(new ApiResponseDto
            {
                Message = "Request rejected successfully",
            });
        }

        //        // POST: api/requests/bulk-assign
        [Authorize("RequireAdminRole")]
        [HttpPost("bulk-assign")]
        public async Task<IActionResult> BulkAssign([FromBody] BulkAssignRequestsCommand command)
        {
            int currentUserId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var cmd = command with { CurrentUserId = currentUserId };
            await _mediator.Send(cmd);

            return Ok(new ApiResponseDto
            {
                Message = "Requests assigned successfully",
            });
        }

        // POST: api/requests/{id}/assign
        [Authorize("RequireAdminRole")]
        [HttpPost("{requestId}/assign")]
        public async Task<IActionResult> Assign(int requestId, [FromBody] AssignRequestDto dto)
        {
            int currentUserId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _mediator.Send(new AssignRequestCommand(requestId, currentUserId, dto));

            return Ok(new ApiResponseDto
            {
                Message = "Request assigned successfully",
            });
        }

        // POST: api/requests/{id}/escalate
        [Authorize("RequireAdminRole")]
        [HttpPost("{id}/escalate")]
        public async Task<IActionResult> Escalate(int id, [FromBody] EscalateRequestDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.EscalationReason))
            {
                return BadRequest(new ApiResponseDto
                {
                    Message = "EscalationReason is required"
                });
            }

            int currentUserId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _mediator.Send(new EscalateRequestCommand(id, currentUserId, dto));

            return Ok(new ApiResponseDto
            {
                Message = "Request escalated successfully",
                Data = result
            });
        }

        //// POST: api/requests/{id}/status
        [Authorize(policy: "RequireSupportRole")]
        [HttpPost("{requestId}/status")]
        public async Task<IActionResult> ChangeStatus(int requestId, UpdateRequestStatusDto dto)
        {
            int currentUserId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            string currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value!;
            await _mediator.Send(new UpdateRequestStatusCommand(requestId, currentUserId, currentUserRole, dto));

            return Ok(new ApiResponseDto
            {
                Message = "Request status updated successfully",
            });
        }

        //// POST: api/requests/{id}/comments
        [Authorize(policy: "CanCreateRequestComment")]
        [HttpPost("{requestId}/comments")]
        public async Task<IActionResult> AddComment(int requestId, AddRequestCommentDto dto)
        {
            int userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _mediator.Send(new AddRequestCommentCommand(requestId, userId, dto));

            return Ok(new ApiResponseDto
            {
                Message = "Comment added successfully",
                Data = result
            });
        }

        //// GET: api/requests/{id}/comments
        [Authorize]
        [HttpGet("{requestId}/comments")]
        public async Task<IActionResult> GetComments(int requestId)
        {
            int userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _mediator.Send(new GetRequestCommentsQuery(requestId, userId));

            return Ok(new ApiResponseDto
            {
                Message = "Comments fetched successfully",
                Data = result
            });
        }

        // GET: api/requests/{id}/history
        [Authorize(policy: "RequireAdminRole")]
        [HttpGet("{requestId}/history")]
        public async Task<IActionResult> GetRequestHistory(int requestId)
        {
            var result = await _mediator.Send(new GetRequestHistoryQuery(requestId));
            return Ok(new ApiResponseDto
            {
                Message = "Request history fetched successfully",
                Data = result
            });
        }
    }
}
