using AutoMapper;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Features.Users.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Users.Queries
{
    public record GetUserByIdQuery(int UserId) : IRequest<UserResponseDto>;

    public class GetUserByIdQueryHandler(IUserRepository userRepository, IMapper _mapper, ILogger<GetUserByIdQueryHandler> logger) : IRequestHandler<GetUserByIdQuery, UserResponseDto>
    {
        public async Task<UserResponseDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            // logging
            logger.LogInformation("Starting {Operation} with {@Request}", nameof(GetUserByIdQueryHandler), request);

            // logging
            logger.LogInformation("Fetching {Entity} with Id {EntityId}", "User", request.UserId);

            var user = await userRepository.GetByIdAsync(request.UserId);

            if (user == null)
            {
                // logging
                logger.LogWarning("{Entity} not found with Id {EntityId} in {Operation}", "User", request.UserId, nameof(GetUserByIdQueryHandler));
                throw new NotFoundException("User not found.");
            }

            // logging
            logger.LogInformation("Mapping {Entity} with Id {EntityId} to DTO", "User", request.UserId);

            var result = _mapper.Map<UserResponseDto>(user);
            result.RoleName = user.Role.RoleName;
            result.ManagerName = user.Manager?.FullName;
            // logging
            logger.LogInformation("Successfully completed {Operation} for {Entity} with Id {EntityId}", nameof(GetUserByIdQueryHandler), "User", request.UserId);

            return result;
        }
    }
}
