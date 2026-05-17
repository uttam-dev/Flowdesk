using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Interfaces;
using MediatR;

namespace FlowDesk.Application.Features.Requests.Commands
{
    public record ApproveRequestCommand(int RequestId, int UserId, RemarkDto Dto) : IRequest;

    public class ApproveRequestCommandHandler(IRequestRepository requestRepository, IRequestHistoryRepository requestHistory, ICommentRepository commentRepository) : IRequestHandler<ApproveRequestCommand>
    {
        async Task IRequestHandler<ApproveRequestCommand>.Handle(ApproveRequestCommand request, CancellationToken cancellationToken)
        {
            var fetchedRequest = await requestRepository.GetByIdAsync(request.RequestId);

            if (fetchedRequest == null)
            {
                throw new NotFoundException($"Request with ID {request.UserId} not found.");
            }

            if (fetchedRequest?.Category?.IsApprovalRequired is false)
            {
                throw new BadRequestException("You are not able to proccess this request");
            }

            var requestHistoryEntry = new Domain.Entities.RequestHistory
            {
                RequestId = fetchedRequest!.RequestId,
                ChangedById = request.UserId,
                OldStatus = fetchedRequest.Status,
                NewStatus = RequestStatusEnum.Approved,
                RemarksId = request.Dto.MasterRemarkId
            };
            fetchedRequest.Status = RequestStatusEnum.Approved;
            await requestRepository.Update(fetchedRequest);
            if (request.Dto.CommentText != null)
            {
                await commentRepository.AddAsync(new Domain.Entities.Comment
                {
                    CommentById = request.UserId,
                    CommentText = request.Dto.CommentText,
                    RequestId = request.RequestId
                });
            }
            await requestHistory.AddAsync(requestHistoryEntry);

        }
    }
}
