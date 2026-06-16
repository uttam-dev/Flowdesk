using FlowDesk.Application.Features.Remote.Commands;
using FlowDesk.Application.Features.Remote.DTOs;
using FlowDesk.Application.Features.Remote.Queries;
using FlowDesk.Domain.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowDesk.Api.Controllers;

[ApiController]
[Route("api/remote-sessions")]
[Authorize]
public class RemoteSessionController(IMediator _mediator) : ControllerBase
{
    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);


    [HttpPost]
    [Authorize(Roles = "Support")]
    public async Task<ActionResult> Initiate(
        [FromBody] InitiateRemoteSessionDto dto,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new InitiateRemoteSessionCommand(dto.RequestId, CurrentUserId), ct);

        return Ok(new ApiResponseDto
        {
            Message = "Remote access request sent. Waiting for the user to accept.",
            Data = result
        });
    }


    [HttpPut("{sessionId:int}/respond")]
    [Authorize(Roles = "Employee,Manager")]
    public async Task<ActionResult> Respond(
        int sessionId,
        [FromBody] RespondToRemoteSessionDto dto,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new RespondToRemoteSessionCommand(
                sessionId,
                CurrentUserId,
                dto.Accepted,
                dto.RejectionReason), ct);

        var message = dto.Accepted
            ? "Remote access accepted. Session is starting."
            : "Remote access rejected.";

        return Ok(new ApiResponseDto
        {
            Message = message,
            Data = result
        });
    }

    [HttpPut("{sessionId:int}/end")]
    [Authorize(Roles = "Support")]
    public async Task<ActionResult> End(
        int sessionId,
        [FromBody] EndRemoteSessionDto dto,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new EndRemoteSessionCommand(
                sessionId,
                CurrentUserId,
                dto.ResolutionNotes,
                dto.ResolveRequest), ct);

        return Ok(new ApiResponseDto
        {
            Message = dto.ResolveRequest
                ? "Session ended and request marked as resolved."
                : "Session ended.",
            Data = result
        });
    }

    
    [HttpGet("{sessionId:int}")]
    public async Task<ActionResult> GetById(
        int sessionId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetRemoteSessionByIdQuery(sessionId), ct);
        return Ok(new ApiResponseDto { Message = "Session fetched successfully.", Data = result });
    }


    [HttpGet("by-request/{requestId:int}/latest")]
    public async Task<ActionResult> GetLatestByRequest(
        int requestId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetLatestRemoteSessionByRequestQuery(requestId), ct);

        return Ok(new ApiResponseDto
        {
            Message = "Latest remote session fetched successfully.",
            Data = result
        });
    }


    [HttpGet("by-request/{requestId:int}/history")]
    public async Task<ActionResult> GetHistory(
        int requestId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetRemoteSessionHistoryQuery(requestId), ct);

        return Ok(new ApiResponseDto
        {
            Message = "Remote session history fetched successfully.",
            Data = result
        });
    }
}
