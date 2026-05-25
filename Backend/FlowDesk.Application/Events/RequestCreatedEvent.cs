using MediatR;

namespace FlowDesk.Application.Events;

/// <summary>
/// Published after a new request is successfully persisted.
/// NOTE: Request creation notifications are currently handled directly inside
/// <c>CreateRequestCommand</c> via <c>IRealtimeService.NotifyRequestCreatedAsync</c>.
/// This event class exists to support a future clean migration to a fully
/// event-driven creation flow without breaking the existing pipeline.
/// </summary>
public record RequestCreatedEvent(
    int RequestId,
    int EmployeeId,
    int? ManagerId,
    string CreatorRole) : INotification;
