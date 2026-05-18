using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Requests.Commands
{
    public record ApproveRequestCommand(int RequestId, int UserId, RemarkDto Dto) : IRequest;

    public class ApproveRequestCommandHandler(
     IRequestRepository requestRepository,
     IRequestHistoryRepository requestHistory,
     ICommentRepository commentRepository,
     ILogger<ApproveRequestCommandHandler> logger)
     : IRequestHandler<ApproveRequestCommand>
    {
        public async Task Handle(ApproveRequestCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting {Operation} with {@Request}", nameof(ApproveRequestCommandHandler), request);

            logger.LogInformation("Fetching Request with Id {RequestId}", request.RequestId);

            var fetchedRequest = await requestRepository.GetByIdAsync(request.RequestId);

            if (fetchedRequest == null)
            {
                logger.LogWarning("Request not found with Id {RequestId}", request.RequestId);
                throw new NotFoundException($"Request with ID {request.UserId} not found.");
            }

            if (fetchedRequest.Category?.IsApprovalRequired is false)
            {
                logger.LogWarning("Approval not allowed for RequestId {RequestId}", request.RequestId);
                throw new BadRequestException("You are not able to proccess this request");
            }

            logger.LogInformation("Approving RequestId {RequestId}", request.RequestId);

            var requestHistoryEntry = new Domain.Entities.RequestHistory
            {
                RequestId = fetchedRequest.RequestId,
                ChangedById = request.UserId,
                OldStatus = fetchedRequest.Status,
                NewStatus = RequestStatusEnum.Approved,
                RemarksId = request.Dto.MasterRemarkId
            };

            fetchedRequest.Status = RequestStatusEnum.Approved;
            await requestRepository.Update(fetchedRequest);

            if (request.Dto.CommentText != null)
            {
                logger.LogInformation("Adding comment for RequestId {RequestId}", request.RequestId);

                await commentRepository.AddAsync(new Domain.Entities.Comment
                {
                    CommentById = request.UserId,
                    CommentText = request.Dto.CommentText,
                    RequestId = request.RequestId
                });
            }

            await requestHistory.AddAsync(requestHistoryEntry);

            logger.LogInformation("Completed {Operation} for RequestId {RequestId}", nameof(ApproveRequestCommandHandler), request.RequestId);
        }
    }
}
