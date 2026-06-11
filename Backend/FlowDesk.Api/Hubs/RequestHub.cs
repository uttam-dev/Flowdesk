using FlowDesk.Application.Features.Remote.Commands;
using FlowDesk.Domain.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace FlowDesk.Api.Hubs;

[Authorize]
public class RequestHub(ILogger<RequestHub> logger, IMediator mediator) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
            logger.LogInformation("Added connection {ConnectionId} to group user-{UserId}",
                Context.ConnectionId, userId);
        }

        if (!string.IsNullOrEmpty(role))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"role-{role}");
            logger.LogInformation("Added connection {ConnectionId} to group role-{Role}",
                Context.ConnectionId, role);
        }

        logger.LogInformation(
            "SignalR connected: ConnectionId={ConnectionId} UserId={UserId} Role={Role}",
            Context.ConnectionId, userId ?? "anonymous", role ?? "none");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (exception is null)
        {
            logger.LogInformation(
                "SignalR disconnected gracefully: ConnectionId={ConnectionId} UserId={UserId}",
                Context.ConnectionId, userId ?? "anonymous");
        }
        else
        {
            logger.LogWarning(exception,
                "SignalR disconnected with error: ConnectionId={ConnectionId} UserId={UserId}",
                Context.ConnectionId, userId ?? "anonymous");
        }

        await base.OnDisconnectedAsync(exception);
    }

    private int GetUserIdFromContext()
    {
        var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }

    public async Task AgentReady(int sessionId)
    {
        var userId = GetUserIdFromContext();

        logger.LogInformation(
            "AgentReady triggered: SessionId={SessionId} UserId={UserId}",
            sessionId, userId);

        await Groups.AddToGroupAsync(Context.ConnectionId, $"session-{sessionId}-agent");
        await Groups.AddToGroupAsync(Context.ConnectionId, $"session-{sessionId}");

        logger.LogInformation(
            "Connection {ConnectionId} joined session groups for SessionId={SessionId}",
            Context.ConnectionId, sessionId);

        await mediator.Send(new ActivateRemoteSessionCommand(sessionId, userId));

        logger.LogInformation(
            "Remote session activated via mediator: SessionId={SessionId} UserId={UserId}",
            sessionId, userId);

        await Clients.Group($"session-{sessionId}-admin")
            .SendAsync("AgentStreamReady", new { sessionId });

        logger.LogInformation(
            "Notified admin group that agent stream is ready: SessionId={SessionId}",
            sessionId);
    }

    public async Task AdminJoinSession(int sessionId)
    {
        var userId = GetUserIdFromContext();

        logger.LogInformation(
            "Admin joining session: SessionId={SessionId} UserId={UserId}",
            sessionId, userId);

        await Groups.AddToGroupAsync(Context.ConnectionId, $"session-{sessionId}-admin");
        await Groups.AddToGroupAsync(Context.ConnectionId, $"session-{sessionId}");

        logger.LogInformation(
            "Connection {ConnectionId} joined admin session groups for SessionId={SessionId}",
            Context.ConnectionId, sessionId);
    }

    public async Task SendWebRTCOffer(int sessionId, string sdpOffer)
    {
        logger.LogInformation(
            "Sending WebRTC Offer: SessionId={SessionId}",
            sessionId);

        await Clients.Group($"session-{sessionId}-admin")
            .SendAsync("ReceiveWebRTCOffer", new { sessionId, sdpOffer });
    }

    public async Task SendWebRTCAnswer(int sessionId, string sdpAnswer)
    {
        logger.LogInformation(
            "Sending WebRTC Answer: SessionId={SessionId}",
            sessionId);

        await Clients.Group($"session-{sessionId}-agent")
            .SendAsync("ReceiveWebRTCAnswer", new { sessionId, sdpAnswer });
    }

    public async Task SendICECandidate(int sessionId, string candidate, bool fromAgent)
    {
        var targetGroup = fromAgent
            ? $"session-{sessionId}-admin"
            : $"session-{sessionId}-agent";

        logger.LogInformation(
            "Sending ICE Candidate: SessionId={SessionId} FromAgent={FromAgent} TargetGroup={TargetGroup}",
            sessionId, fromAgent, targetGroup);

        await Clients.Group(targetGroup)
            .SendAsync("ReceiveICECandidate", new { sessionId, candidate });
    }

    public async Task SendControlEvent(int sessionId, ControlEventDto controlEvent)
    {
        logger.LogInformation(
            "Sending ControlEvent: SessionId={SessionId} EventType={EventType}",
            sessionId, controlEvent?.Type);

        await Clients.Group($"session-{sessionId}-agent")
            .SendAsync("ExecuteControlEvent", controlEvent);
    }
}