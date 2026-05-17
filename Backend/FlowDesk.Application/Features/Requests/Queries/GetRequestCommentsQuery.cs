using AutoMapper;
using FlowDesk.Application.Common.Validators;
using FlowDesk.Application.Features.Requests.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Requests.Queries
{
    public record GetRequestCommentsQuery(int RequestId, int CurrentUserId)
     : IRequest<List<CommentResponseDto>>;
    public class GetRequestCommentsQueryHandler(
        IRequestRepository requestRepository,
        ICommentRepository commentRepository)
        : IRequestHandler<GetRequestCommentsQuery, List<CommentResponseDto>>
    {
        public async Task<List<CommentResponseDto>> Handle(
            GetRequestCommentsQuery request,
            CancellationToken cancellationToken)
        {
            var req = await requestRepository.GetByIdAsync(request.RequestId);
            if (req == null)
            {
                throw new Common.Exceptions.NotFoundException("Request not found.");
            }

            var comments = await commentRepository.GetByRequestIdAsync(request.RequestId);

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

            return result;
        }
    }
}
