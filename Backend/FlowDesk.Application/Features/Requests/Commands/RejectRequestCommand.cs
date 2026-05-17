using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Requests.Commands
{
    public record RejectRequestCommand(int RequestId, int UserId, RemarkDto Dto) : IRequest;

    public class RejectRequestCommandHandler(IRequestRepository requestRepository,
        ICommentRepository commentRepository,
        IRequestHistoryRepository requestHistoryRepository) : IRequestHandler<RejectRequestCommand>
    {
        public async Task Handle(RejectRequestCommand request, CancellationToken cancellationToken)
        {
            if (request.Dto == null || request.Dto.MasterRemarkId <= 0)
            {
                throw new ArgumentException("Remarks is required.");

            }

            if (request.Dto.CommentText == null)
            {
                throw new ArgumentException("Comment is required.");
            }
            var fetchedRequest = await requestRepository.GetByIdAsync(request.RequestId);
            if (fetchedRequest == null)
            {
                throw new NotFoundException($"Request with ID {request.UserId} not found.");
            }

            if (fetchedRequest.Category!.IsApprovalRequired == false)
            {
                throw new BadRequestException("You are not able to reject this request.");
            }

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
            await requestRepository.Update(fetchedRequest);
        }
    }
}
