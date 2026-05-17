using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Features.Requests.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Requests.Commands
{
    public record AssignRequestCommand(int RequestId, int CurrentUserId, AssignRequestDto Dto) : IRequest;
    public class AssignRequestCommandHandler(IUserRepository userRepository,
        IRequestRepository requestRepository,
        ICommentRepository commentRepository,
        IRequestHistoryRepository requestHistoryRepository) : IRequestHandler<AssignRequestCommand>
    {
        public async Task Handle(AssignRequestCommand request, CancellationToken cancellationToken)
        {
            var fetchedRequest = await requestRepository.GetByIdAsync(request.RequestId);

            if (fetchedRequest == null)
            {
                throw new BadRequestException("Invalid request id");
            }
            var supportUser = await userRepository.GetByIdAsync(request.Dto.AssignToId);
            if (supportUser == null)
            {
                throw new BadRequestException("Invalid assigner id");
            }

            await requestHistoryRepository.AddAsync(new Domain.Entities.RequestHistory
            {
                RequestId = fetchedRequest.RequestId,
                ChangedById = request.CurrentUserId,
                OldStatus = fetchedRequest.Status,
                NewStatus = Domain.Enums.RequestStatusEnum.Assigned,
                RemarksId = request.Dto.RemarksId == 0 ? null : request.Dto.RemarksId,
            });

            fetchedRequest.Status = Domain.Enums.RequestStatusEnum.Assigned;
            fetchedRequest.AssignedToId = request.Dto.AssignToId;
            await requestRepository.Update(fetchedRequest);
            if (request.Dto.CommentText != null)
            {
                await commentRepository.AddAsync(new Domain.Entities.Comment
                {
                    CommentById = request.CurrentUserId,
                    CommentText = request.Dto.CommentText,
                    RequestId = fetchedRequest.RequestId
                });
            }
        }
    }
}
