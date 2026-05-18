using FlowDesk.Application.Features.Requests.DTOs;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Interfaces;
using FlowDesk.Domain.Utils;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Requests.Commands
{
    public record UpdateRequestStatusCommand(int RequestId, int UserId, string RoleName, UpdateRequestStatusDto Dto) : IRequest;

    public class UpdateRequestStatusCommandHandler(
        IRequestRepository requestRepository,
        ICommentRepository commentRepository,
        IRequestHistoryRepository historyRepository,
        ILogger<UpdateRequestStatusCommandHandler> logger)
        : IRequestHandler<UpdateRequestStatusCommand>
    {
        public async Task Handle(UpdateRequestStatusCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting {Operation} with {@Request}", nameof(UpdateRequestStatusCommandHandler), request);

            var req = await requestRepository.GetByIdAsync(request.RequestId);

            if (req == null)
            {
                logger.LogWarning("Request not found with Id {RequestId}", request.RequestId);
                throw new Common.Exceptions.NotFoundException("Request not found.");
            }

            if (request.RoleName != RoleName.Support)
            {
                logger.LogWarning("Unauthorized role {Role} for RequestId {RequestId}", request.RoleName, request.RequestId);
                throw new Common.Exceptions.UnauthorizedException("Only support can update status.");
            }

            if (req.AssignedToId != request.UserId)
            {
                logger.LogWarning("User {UserId} not assigned to RequestId {RequestId}", request.UserId, request.RequestId);
                throw new Common.Exceptions.UnauthorizedException("You are not assigned to this request.");
            }

            var newStatus = request.Dto.Status;

            if (newStatus != RequestStatusEnum.InProgress &&
                newStatus != RequestStatusEnum.Resolved)
            {
                logger.LogWarning("Invalid status {Status} for RequestId {RequestId}", newStatus, request.RequestId);
                throw new Common.Exceptions.BadRequestException("Invalid status.");
            }

            if (newStatus == RequestStatusEnum.InProgress &&
                req.Status != RequestStatusEnum.Assigned)
            {
                logger.LogWarning("Invalid transition to InProgress for RequestId {RequestId}", request.RequestId);
                throw new Common.Exceptions.BadRequestException("Only assigned request can move to InProgress.");
            }

            if (newStatus == RequestStatusEnum.Resolved &&
                req.Status != RequestStatusEnum.InProgress)
            {
                logger.LogWarning("Invalid transition to Resolved for RequestId {RequestId}", request.RequestId);
                throw new Common.Exceptions.BadRequestException("Only InProgress request can be resolved.");
            }

            var oldStatus = req.Status;

            logger.LogInformation("Updating RequestId {RequestId} from {OldStatus} to {NewStatus}", request.RequestId, oldStatus, newStatus);

            req.Status = newStatus;
            req.UpdatedOn = DateTime.UtcNow;

            await requestRepository.Update(req);

            await historyRepository.AddAsync(new RequestHistory
            {
                RequestId = req.RequestId,
                ChangedById = request.UserId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                RemarksId = request.Dto.RemarksId,
            });

            if (request.Dto.CommentText != null)
            {
                logger.LogInformation("Adding comment for RequestId {RequestId}", request.RequestId);

                await commentRepository.AddAsync(new Comment
                {
                    CommentById = request.UserId,
                    CommentText = request.Dto.CommentText,
                    RequestId = req.RequestId
                });
            }

            if (newStatus == RequestStatusEnum.Resolved)
            {
                logger.LogInformation("Auto-closing RequestId {RequestId}", request.RequestId);

                await historyRepository.AddAsync(new RequestHistory
                {
                    RequestId = req.RequestId,
                    OldStatus = RequestStatusEnum.Resolved,
                    NewStatus = RequestStatusEnum.Closed,
                    IsSystemGenerated = true
                });
            }

            logger.LogInformation("Completed {Operation} for RequestId {RequestId}", nameof(UpdateRequestStatusCommandHandler), request.RequestId);
        }
    }
}
