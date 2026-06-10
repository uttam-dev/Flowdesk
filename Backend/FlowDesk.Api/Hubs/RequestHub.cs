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
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

        if (!string.IsNullOrEmpty(role))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"role-{role}");

        logger.LogInformation(
            "SignalR connected: ConnectionId={ConnectionId} UserId={UserId} Role={Role}",
            Context.ConnectionId, userId ?? "anonymous", role ?? "none");

        await base.OnConnectedAsync();
    }


    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (exception is null)
            logger.LogInformation(
                "SignalR disconnected gracefully: ConnectionId={ConnectionId} UserId={UserId}",
                Context.ConnectionId, userId ?? "anonymous");
        else
            logger.LogWarning(exception,
                "SignalR disconnected with error: ConnectionId={ConnectionId} UserId={UserId}",
                Context.ConnectionId, userId ?? "anonymous");

        await base.OnDisconnectedAsync(exception);
    }
    private int GetUserIdFromContext()
    {
        var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }

    public async Task AgentReady(int sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"session-{sessionId}-agent");
        await Groups.AddToGroupAsync(Context.ConnectionId, $"session-{sessionId}");

        // Tell the system the WebRTC stream is about to start
        var userId = GetUserIdFromContext();
        await mediator.Send(new ActivateRemoteSessionCommand(sessionId, userId));

        // Notify admin side to prepare the video element
        await Clients.Group($"session-{sessionId}-admin")
            .SendAsync("AgentStreamReady", new { sessionId });
    }


    public async Task AdminJoinSession(int sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"session-{sessionId}-admin");
        await Groups.AddToGroupAsync(Context.ConnectionId, $"session-{sessionId}");
    }


    public async Task SendWebRTCOffer(int sessionId, string sdpOffer)
    {
        await Clients.Group($"session-{sessionId}-admin")
            .SendAsync("ReceiveWebRTCOffer", new { sessionId, sdpOffer });
    }


    public async Task SendWebRTCAnswer(int sessionId, string sdpAnswer)
    {
        await Clients.Group($"session-{sessionId}-agent")
            .SendAsync("ReceiveWebRTCAnswer", new { sessionId, sdpAnswer });
    }


    public async Task SendICECandidate(int sessionId, string candidate, bool fromAgent)
    {
        var targetGroup = fromAgent
            ? $"session-{sessionId}-admin"
            : $"session-{sessionId}-agent";

        await Clients.Group(targetGroup)
            .SendAsync("ReceiveICECandidate", new { sessionId, candidate });
    }


    public async Task SendControlEvent(int sessionId, ControlEventDto controlEvent)
    {
        await Clients.Group($"session-{sessionId}-agent")
            .SendAsync("ExecuteControlEvent", controlEvent);
    }
}