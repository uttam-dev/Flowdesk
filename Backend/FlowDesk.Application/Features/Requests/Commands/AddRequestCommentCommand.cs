using AutoMapper;
using FlowDesk.Application.Features.Requests.DTOs;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Interfaces;
using MediatR;

namespace FlowDesk.Application.Features.Requests.Commands
{
    public record AddRequestCommentCommand(int reqestId, int userId, AddRequestCommentDto Dto) : IRequest<CommentResponseDto>;

    public class AddRequestCommentCommandHandler(
    IRequestRepository requestRepository,
    ICommentRepository commentRepository,
    IUserRepository userRepository,
    IMapper mapper)
    : IRequestHandler<AddRequestCommentCommand, CommentResponseDto>
    {
        public async Task<CommentResponseDto> Handle(
            AddRequestCommentCommand request,
            CancellationToken cancellationToken)
        {
            // Validate Request exists
            var fetchedRequest = await requestRepository.GetByIdAsync(request.reqestId);
            if (fetchedRequest == null)
            {
                throw new Common.Exceptions.NotFoundException("Request not found.");
            }

            // Validate User exists
            var user = await userRepository.GetByIdAsync(request.userId);
            if (user == null)
            {
                throw new Common.Exceptions.NotFoundException("User not found.");
            }

            // Validate Comment text
            if (string.IsNullOrWhiteSpace(request.Dto.CommentText))
            {
                throw new Common.Exceptions.BadRequestException("Comment cannot be empty.");
            }

            // Create Comment
            var comment = new Comment
            {
                RequestId = fetchedRequest.RequestId,
                CommentById = request.userId,
                CommentText = request.Dto.CommentText!,
            };

            var createdComment = await commentRepository.AddAsync(comment);

            // Map response
            var response = mapper.Map<CommentResponseDto>(createdComment);
            response.CommentByName = user.FullName;
            response.CommentByRoleName = user.Role.RoleName;
            response.IsCurrentUser = true;

            return response;
        }
    }
}
