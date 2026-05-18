using AutoMapper;
using FlowDesk.Application.Features.Requests.DTOs;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Requests.Commands
{
    public record AddRequestCommentCommand(int reqestId, int userId, AddRequestCommentDto Dto) : IRequest<CommentResponseDto>;

    public class AddRequestCommentCommandHandler(
    IRequestRepository requestRepository,
    ICommentRepository commentRepository,
    IUserRepository userRepository,
    IMapper mapper,
    ILogger<AddRequestCommentCommandHandler> logger)
    : IRequestHandler<AddRequestCommentCommand, CommentResponseDto>
    {
        public async Task<CommentResponseDto> Handle(
            AddRequestCommentCommand request,
            CancellationToken cancellationToken)
        {
            // logging
            logger.LogInformation("Starting {Operation} with {@Request}", nameof(AddRequestCommentCommandHandler), request);

            // logging
            logger.LogInformation("Fetching Request with Id {RequestId}", request.reqestId);

            var fetchedRequest = await requestRepository.GetByIdAsync(request.reqestId);
            if (fetchedRequest == null)
            {
                logger.LogWarning("Request not found with Id {RequestId}", request.reqestId);
                throw new Common.Exceptions.NotFoundException("Request not found.");
            }

            // logging
            logger.LogInformation("Fetching User with Id {UserId}", request.userId);

            var user = await userRepository.GetByIdAsync(request.userId);
            if (user == null)
            {
                logger.LogWarning("User not found with Id {UserId}", request.userId);
                throw new Common.Exceptions.NotFoundException("User not found.");
            }

            if (string.IsNullOrWhiteSpace(request.Dto.CommentText))
            {
                logger.LogWarning("Empty comment attempt by UserId {UserId}", request.userId);
                throw new Common.Exceptions.BadRequestException("Comment cannot be empty.");
            }

            // logging
            logger.LogInformation("Creating comment for RequestId {RequestId}", fetchedRequest.RequestId);

            var comment = new Comment
            {
                RequestId = fetchedRequest.RequestId,
                CommentById = request.userId,
                CommentText = request.Dto.CommentText!,
            };

            var createdComment = await commentRepository.AddAsync(comment);

            // logging
            logger.LogInformation("Mapping response for RequestId {RequestId}", fetchedRequest.RequestId);

            var response = mapper.Map<CommentResponseDto>(createdComment);
            response.CommentByName = user.FullName;
            response.CommentByRoleName = user.Role.RoleName;
            response.IsCurrentUser = true;

            logger.LogInformation("Completed {Operation} for RequestId {RequestId}", nameof(AddRequestCommentCommandHandler), fetchedRequest.RequestId);

            return response;
        }
    }
}
