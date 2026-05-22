using FlowDesk.Api.Hubs;
using FlowDesk.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace FlowDesk.Api.Services.Realtime;

public class SignalRService(IHubContext<RequestHub> _hub) : IRealtimeService
{
    public async Task NotifyRequestCreatedAsync(int requestId, int employeeId, int? managerId)
    {
        var payload = new
        {
            RequestId = requestId,
            Action = "CREATED",
            Timestamp = DateTime.UtcNow
        };

        var tasks = new List<Task>
        {
            _hub.Clients.Group($"user-{employeeId}")
                .SendAsync("RequestUpdated", payload),

            _hub.Clients.Group("role-Admin")
                .SendAsync("RequestUpdated", payload)
        };

        if (managerId.HasValue)
        {
            tasks.Add(_hub.Clients.Group($"user-{managerId.Value}")
                .SendAsync("RequestUpdated", payload));
        }

        await Task.WhenAll(tasks);
    }

    public async Task NotifyRequestUpdatedAsync(int requestId, string action, IEnumerable<int> userIds)
    {
        var payload = new
        {
            RequestId = requestId,
            Action = action,
            Timestamp = DateTime.UtcNow
        };

        var tasks = userIds.Select(userId =>
            _hub.Clients.Group($"user-{userId}")
                .SendAsync("RequestUpdated", payload)
        );

        await Task.WhenAll(tasks);
    }
}