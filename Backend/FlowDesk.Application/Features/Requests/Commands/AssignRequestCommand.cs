using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Features.Requests.DTOs;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Requests.Commands
{
    public record AssignRequestCommand(int RequestId, int CurrentUserId, AssignRequestDto Dto) : IRequest;
    public class AssignRequestCommandHandler(
     IUserRepository userRepository,
     IRequestRepository requestRepository,
     ICommentRepository commentRepository,
     IRequestHistoryRepository requestHistoryRepository,
     ILogger<AssignRequestCommandHandler> logger)
     : IRequestHandler<AssignRequestCommand>
    {
        public async Task Handle(AssignRequestCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting {Operation} with {@Request}", nameof(AssignRequestCommandHandler), request);

            logger.LogInformation("Fetching Request with Id {RequestId}", request.RequestId);

            var fetchedRequest = await requestRepository.GetByIdAsync(request.RequestId);

            if (fetchedRequest == null)
            {
                logger.LogWarning("Invalid RequestId {RequestId}", request.RequestId);
                throw new BadRequestException("Invalid request id");
            }

            var IsManager = fetchedRequest?.Employee?.Manager == null;
            var IsEmployee = fetchedRequest?.Employee?.Manager != null;

            if (IsManager && fetchedRequest?.Status != RequestStatusEnum.Open)
            {
                logger.LogWarning("Request {RequestId} is not in Open state. Current Status: {Status}",
                    fetchedRequest!.RequestId, fetchedRequest.Status);

                throw new BadRequestException("Invalid request.");
            }

            if (IsEmployee &&
                fetchedRequest?.Category?.IsApprovalRequired == true &&
                fetchedRequest.Status != RequestStatusEnum.Approved)
            {
                logger.LogWarning("Request {RequestId} required approval. Current Status: {Status}",
                    fetchedRequest.RequestId, fetchedRequest.Status);
                throw new BadRequestException("Invalid request.");
            }

            if (IsEmployee &&
                fetchedRequest?.Category?.IsApprovalRequired != true &&
                fetchedRequest!.Status != RequestStatusEnum.Open)
            {
                logger.LogWarning("Request {RequestId} is only procced when its open., Current Status: {Status}",
                    fetchedRequest.RequestId, fetchedRequest.Status);
                throw new BadRequestException("Invalid request.");
            }


            logger.LogInformation("Fetching User with Id {UserId}", request.Dto.AssignToId);

            var supportUser = await userRepository.GetByIdAsync(request.Dto.AssignToId);

            if (supportUser == null)
            {
                logger.LogWarning("Invalid AssignToId {UserId}", request.Dto.AssignToId);
                throw new BadRequestException("Invalid assigner id");
            }

            logger.LogInformation("Assigning RequestId {RequestId} to UserId {UserId}", request.RequestId, request.Dto.AssignToId);

            await requestHistoryRepository.AddAsync(new Domain.Entities.RequestHistory
            {
                RequestId = fetchedRequest!.RequestId,
                ChangedById = request.CurrentUserId,
                OldStatus = fetchedRequest.Status,
                NewStatus = Domain.Enums.RequestStatusEnum.Assigned,
                RemarksId = request.Dto.RemarksId == 0 || request.Dto.RemarksId == null ? null : request.Dto.RemarksId,
            });

            fetchedRequest.Status = Domain.Enums.RequestStatusEnum.Assigned;
            fetchedRequest.AssignedToId = request.Dto.AssignToId;
            await requestRepository.Update(fetchedRequest);

            if (request.Dto.CommentText != null)
            {
                logger.LogInformation("Adding comment for RequestId {RequestId}", request.RequestId);

                await commentRepository.AddAsync(new Domain.Entities.Comment
                {
                    CommentById = request.CurrentUserId,
                    CommentText = request.Dto.CommentText,
                    RequestId = fetchedRequest.RequestId
                });
            }

            logger.LogInformation("Completed {Operation} for RequestId {RequestId}", nameof(AssignRequestCommandHandler), request.RequestId);
        }
    }
}
