using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Events;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Requests.Commands
{
    public record BulkAssignRequestsCommand(
        int CurrentUserId,
        List<int> RequestIds,
        int AssignToId,
        int? RemarksId,
        string? CommentText) : IRequest;

    public class BulkAssignRequestsCommandHandler(
        IUserRepository userRepository,
        IRequestRepository requestRepository,
        IRequestHistoryRepository requestHistoryRepository,
        IPublisher publisher,
        ILogger<BulkAssignRequestsCommandHandler> logger)
        : IRequestHandler<BulkAssignRequestsCommand>
    {
        public async Task Handle(BulkAssignRequestsCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation(
                "Starting {Operation}: assigning {Count} requests to UserId={AssignToId}",
                nameof(BulkAssignRequestsCommandHandler), request.RequestIds.Count, request.AssignToId);

            // ── Validate support user ──────────────────────────────────────────
            var supportUser = await userRepository.GetByIdAsync(request.AssignToId);
            if (supportUser == null)
            {
                logger.LogWarning("Invalid AssignToId {UserId}", request.AssignToId);
                throw new BadRequestException("Invalid assignee id");
            }

            // ── Fetch all target requests ──────────────────────────────────────
            var requests = await requestRepository.GetByIdsAsync(request.RequestIds);

            var notFound = request.RequestIds
                .Select(id => (Id: id, Found: requests.Any(r => r.RequestId == id)))
                .Where(x => !x.Found)
                .Select(x => x.Id)
                .ToList();

            if (notFound.Count != 0)
            {
                logger.LogWarning("Requests not found: {Ids}", notFound);
                throw new BadRequestException($"Requests not found: {string.Join(", ", notFound)}");
            }

            var remarksId = request.RemarksId is 0 ? null : request.RemarksId;

            // ── Process each request ───────────────────────────────────────────
            foreach (var fetchedRequest in requests)
            {
                var isManager = fetchedRequest.Employee?.Manager == null;
                var isEmployee = fetchedRequest.Employee?.Manager != null;

                // Reuse same validation rules as single-assign handler
                if (isManager && fetchedRequest.Status != RequestStatusEnum.Open)
                {
                    logger.LogWarning(
                        "Request {RequestId} is not in Open state. Current Status: {Status}",
                        fetchedRequest.RequestId, fetchedRequest.Status);
                    throw new BadRequestException($"Request #{fetchedRequest.RequestNumber} (ID: {fetchedRequest.RequestId}) cannot be assigned: it must be in Open state.");
                }

                if (isEmployee &&
                    fetchedRequest.Category?.IsApprovalRequired == true &&
                    fetchedRequest.Status != RequestStatusEnum.Approved)
                {
                    logger.LogWarning(
                        "Request {RequestId} required approval. Current Status: {Status}",
                        fetchedRequest.RequestId, fetchedRequest.Status);
                    throw new BadRequestException($"Request #{fetchedRequest.RequestNumber} (ID: {fetchedRequest.RequestId}) requires approval before assignment.");
                }

                if (isEmployee &&
                    fetchedRequest.Category?.IsApprovalRequired != true &&
                    fetchedRequest.Status != RequestStatusEnum.Open)
                {
                    logger.LogWarning(
                        "Request {RequestId} is only proceeded when open. Current Status: {Status}",
                        fetchedRequest.RequestId, fetchedRequest.Status);
                    throw new BadRequestException($"Request #{fetchedRequest.RequestNumber} (ID: {fetchedRequest.RequestId}) must be in Open state.");
                }

                logger.LogInformation(
                    "Assigning RequestId {RequestId} to UserId {UserId}",
                    fetchedRequest.RequestId, request.AssignToId);

                // ── History record ─────────────────────────────────────────────
                await requestHistoryRepository.AddAsync(new Domain.Entities.RequestHistory
                {
                    RequestId = fetchedRequest.RequestId,
                    ChangedById = request.CurrentUserId,
                    OldStatus = fetchedRequest.Status,
                    NewStatus = RequestStatusEnum.Assigned,
                    RemarksId = remarksId,
                });

                // ── Update entity ──────────────────────────────────────────────
                fetchedRequest.Status = RequestStatusEnum.Assigned;
                fetchedRequest.AssignedToId = request.AssignToId;
                await requestRepository.Update(fetchedRequest);

                // ── Real-time notification ─────────────────────────────────────
                await publisher.Publish(new RequestAssignedEvent(
                    RequestId: fetchedRequest.RequestId,
                    AssignedToId: request.AssignToId,
                    AssignedToName: supportUser.FullName ?? string.Empty,
                    EmployeeId: fetchedRequest.EmployeeId,
                    ManagerId: fetchedRequest.Employee?.ManagerId), cancellationToken);
            }

            logger.LogInformation(
                "Completed {Operation}: {Count} requests assigned to UserId={AssignToId}",
                nameof(BulkAssignRequestsCommandHandler), request.RequestIds.Count, request.AssignToId);
        }
    }
}
