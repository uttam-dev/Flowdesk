using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Requests.Commands
{
    public record RejectRequestCommand(int RequestId, int UserId, RemarkDto Dto) : IRequest;

    public class RejectRequestCommandHandler(
        IRequestRepository requestRepository,
        ICommentRepository commentRepository,
        IRequestHistoryRepository requestHistoryRepository,
        ILogger<RejectRequestCommandHandler> logger)
        : IRequestHandler<RejectRequestCommand>
    {
        public async Task Handle(RejectRequestCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting {Operation} with {@Request}", nameof(RejectRequestCommandHandler), request);

            if (request.Dto == null || request.Dto.MasterRemarkId <= 0)
            {
                logger.LogWarning("Invalid remarks provided");
                throw new ArgumentException("Remarks is required.");
            }

            if (request.Dto.CommentText == null)
            {
                logger.LogWarning("Comment is missing for rejection");
                throw new ArgumentException("Comment is required.");
            }

            logger.LogInformation("Fetching Request with Id {RequestId}", request.RequestId);
            var fetchedRequest = await requestRepository.GetByIdAsync(request.RequestId);

            if (fetchedRequest == null)
            {
                logger.LogWarning("Request not found with Id {RequestId}", request.RequestId);
                throw new NotFoundException($"Request with ID {request.UserId} not found.");
            }

            if (fetchedRequest.Category!.IsApprovalRequired == false)
            {
                logger.LogWarning("Reject not allowed for RequestId {RequestId}", request.RequestId);
                throw new BadRequestException("You are not able to reject this request.");
            }

            logger.LogInformation("Rejecting RequestId {RequestId}", request.RequestId);

            await requestHistoryRepository.AddAsync(new Domain.Entities.RequestHistory
            {
                RequestId = fetchedRequest.RequestId,
                ChangedById = request.UserId,
                OldStatus = fetchedRequest.Status,
                NewStatus = Domain.Enums.RequestStatusEnum.Rejected,
                RemarksId = request.Dto.MasterRemarkId
            });

            await commentRepository.AddAsync(new Domain.Entities.Comment
            {
                CommentById = request.UserId,
                CommentText = request.Dto.CommentText,
                RequestId = fetchedRequest.RequestId
            });

            fetchedRequest.Status = Domain.Enums.RequestStatusEnum.Rejected;
            fetchedRequest.UpdatedOn = DateTime.UtcNow;

            await requestRepository.Update(fetchedRequest);

            logger.LogInformation("Completed {Operation} for RequestId {RequestId}", nameof(RejectRequestCommandHandler), request.RequestId);
        }
    }
}
