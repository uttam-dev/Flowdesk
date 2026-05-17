using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Interfaces;
using FlowDesk.Domain.Utils;
using FlowDesk.Application.Features.Requests.DTOs;
using MediatR;

namespace FlowDesk.Application.Features.Requests.Commands
{
    public record UpdateRequestStatusCommand(int RequestId, int UserId, string RoleName, UpdateRequestStatusDto Dto) : IRequest;

    public class UpdateRequestStatusCommandHandler(
                IRequestRepository requestRepository,
                IRequestHistoryRepository historyRepository)
                : IRequestHandler<UpdateRequestStatusCommand>
    {
        public async Task Handle(UpdateRequestStatusCommand request, CancellationToken cancellationToken)
        {
            var req = await requestRepository.GetByIdAsync(request.RequestId);

            if (req == null)
                throw new Common.Exceptions.NotFoundException("Request not found.");

            // Only SUPPORT allowed here
            if (request.RoleName != RoleName.Support)
                throw new Common.Exceptions.UnauthorizedException("Only support can update status.");

            // Only assigned support can update
            if (req.AssignedToId != request.UserId)
                throw new Common.Exceptions.UnauthorizedException("You are not assigned to this request.");

            var newStatus = request.Dto.Status;

            // VALIDATION (IMPORTANT)
            if (newStatus != RequestStatusEnum.InProgress &&
                newStatus != RequestStatusEnum.Resolved)
            {
                throw new Common.Exceptions.BadRequestException("Invalid status.");
            }

            // FLOW VALIDATION
            if (newStatus == RequestStatusEnum.InProgress &&
                req.Status != RequestStatusEnum.Assigned)
            {
                throw new Common.Exceptions.BadRequestException("Only assigned request can move to InProgress.");
            }

            if (newStatus == RequestStatusEnum.Resolved &&
                req.Status != RequestStatusEnum.InProgress)
            {
                throw new Common.Exceptions.BadRequestException("Only InProgress request can be resolved.");
            }

            var oldStatus = req.Status;

            // UPDATE
            req.Status = newStatus;
            req.UpdatedOn = DateTime.UtcNow;

            await requestRepository.Update(req);

            // HISTORY
            await historyRepository.AddAsync(new RequestHistory
            {
                RequestId = req.RequestId,
                ChangedById = request.UserId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                RemarksId = request.Dto.RemarksId,
            });

            //System update history
            if (newStatus == RequestStatusEnum.Resolved)
            {
                await historyRepository.AddAsync(new RequestHistory
                {
                    RequestId = req.RequestId,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    IsSystemGenerated = true,
                });
            }
        }
    }
}
