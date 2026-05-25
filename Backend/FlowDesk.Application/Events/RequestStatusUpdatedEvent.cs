using MediatR;

namespace FlowDesk.Application.Events;

/// <summary>
/// Published by a pipeline behavior after any request status-change command
/// (Approve, Reject, UpdateRequestStatus) completes successfully.
/// <para>
///   Recipients resolved by <c>RequestStatusUpdatedEventHandler</c>:
///   <list type="bullet">
///     <item><c>role-Admin</c> — always notified</item>
///     <item><c>user-{EmployeeId}</c> — the request creator</item>
///     <item><c>user-{AssignedToId}</c> — the assigned support user (if any)</item>
///     <item><c>user-{ManagerId}</c> — the employee's manager (if any)</item>
///   </list>
/// </para>
/// </summary>
public record RequestStatusUpdatedEvent(
    int RequestId,
    string NewStatus,
    int UpdatedById,
    int EmployeeId,
    int? AssignedToId,
    int? ManagerId) : INotification;
