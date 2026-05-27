using MediatR;

namespace FlowDesk.Application.Events;

/// <summary>
/// Published by <see cref="Behaviors.EscalateRequestNotificationBehavior"/> AFTER
/// <c>EscalateRequestCommandHandler</c> completes successfully and the DB write is committed.
/// <para>
///   Recipients resolved by <see cref="EventHandlers.RequestEscalatedEventHandler"/>:
///   <list type="bullet">
///     <item><c>user-{EmployeeId}</c>    — the request creator is informed of the escalation</item>
///     <item><c>user-{ManagerId}</c>     — the employee's manager (if any)</item>
///     <item><c>user-{AssignedToId}</c>  — the support user currently working on it (if any)</item>
///     <item><c>role-Admin</c>           — admins are always kept in sync</item>
///   </list>
/// </para>
/// </summary>
public record RequestEscalatedEvent(
    int RequestId,
    int EmployeeId,
    int? ManagerId,
    int? AssignedToId,
    int EscalatedById) : INotification;
