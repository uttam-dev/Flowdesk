using AutoMapper;
using FlowDesk.Application.Common.Validators;
using FlowDesk.Application.Features.Requests.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Requests.Queries
{
    public record GetRequestCommentsQuery(int RequestId, int CurrentUserId)
     : IRequest<List<CommentResponseDto>>;
    public class GetRequestCommentsQueryHandler(
      IRequestRepository requestRepository,
      ICommentRepository commentRepository,
      ILogger<GetRequestCommentsQueryHandler> logger)
      : IRequestHandler<GetRequestCommentsQuery, List<CommentResponseDto>>
    {
        public async Task<List<CommentResponseDto>> Handle(
            GetRequestCommentsQuery request,
            CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting {Operation} for RequestId {RequestId}",
                nameof(GetRequestCommentsQueryHandler), request.RequestId);

            var req = await requestRepository.GetByIdAsync(request.RequestId);

            if (req == null)
            {
                logger.LogWarning("Request not found with Id {RequestId}", request.RequestId);
                throw new Common.Exceptions.NotFoundException("Request not found.");
            }

            var comments = await commentRepository.GetByRequestIdAsync(request.RequestId);

            logger.LogInformation("Fetched {Count} comments for RequestId {RequestId}",
                comments.Count(), request.RequestId);

            var result = comments
                .OrderBy(c => c.CreatedOn)
                .Select(c => new CommentResponseDto
                {
                    CommentId = c.CommentId,
                    CommentText = c.CommentText,
                    CommentByName = c.Commenter!.FullName,
                    CreatedOn = c.CreatedOn,
                    IsCurrentUser = c.CommentById == request.CurrentUserId,
                    RequestId = c.RequestId,
                    CommentByRoleName = c.Commenter.Role.RoleName
                })
                .ToList();

            logger.LogInformation("Completed {Operation} for RequestId {RequestId}",
                nameof(GetRequestCommentsQueryHandler), request.RequestId);

            return result;
        }
    }
}
