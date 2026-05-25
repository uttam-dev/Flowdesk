using MediatR;

namespace FlowDesk.Application.Events;

/// <summary>
/// Published by a pipeline behavior after <c>AssignRequestCommand</c> completes.
/// <para>
///   Recipients resolved by <c>RequestAssignedEventHandler</c>:
///   <list type="bullet">
///     <item><c>role-Support</c> — all online support agents see the new work item</item>
///     <item><c>user-{AssignedToId}</c> — the specific support user who was assigned</item>
///     <item><c>role-Admin</c> — admins are always kept in sync</item>
///   </list>
/// </para>
/// </summary>
public record RequestAssignedEvent(
    int RequestId,
    int AssignedToId,
    string AssignedToName,
    int EmployeeId,
    int? ManagerId) : INotification;
