using FlowDesk.Application.Features.Users.DTOs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Users.Commands
{
    public record UpdateUserDetailsCommand(int userId, UpdateUserDetailsDto userDto)
     : MediatR.IRequest<UserResponseDto>;

    public class UpdateUserDetailsCommandHandler(
         Domain.Interfaces.IUserRepository _userRepository,
         AutoMapper.IMapper _mapper,
         ILogger<UpdateUserDetailsCommandHandler> logger)
     : MediatR.IRequestHandler<UpdateUserDetailsCommand, UserResponseDto>
    {
        public async Task<UserResponseDto> Handle(
            UpdateUserDetailsCommand request,
            CancellationToken cancellationToken)
        {
            // logging
            logger.LogInformation("Starting {Operation} with {@Request}", nameof(UpdateUserDetailsCommandHandler), request);

            // logging
            logger.LogInformation("Fetching {Entity} with Id {EntityId}", "User", request.userId);

            var user = await _userRepository.GetByIdAsync(request.userId);

            if (user == null)
            {
                // logging
                logger.LogWarning("{Entity} not found with Id {EntityId} in {Operation}", "User", request.userId, nameof(UpdateUserDetailsCommandHandler));
                throw new Common.Exceptions.NotFoundException("User not found.");
            }

            // logging
            logger.LogInformation("Mapping updates to {Entity} with Id {EntityId}", "User", request.userId);

            // Only updates non-null fields from DTO
            _mapper.Map(request.userDto, user);
            user.UpdatedOn = DateTime.UtcNow;

            // logging
            logger.LogInformation("Updating {Entity} with Id {EntityId}", "User", request.userId);

            var updatedUser = await _userRepository.Update(user);

            // logging
            logger.LogInformation("Successfully completed {Operation} for {Entity} with Id {EntityId}", nameof(UpdateUserDetailsCommandHandler), "User", request.userId);

            return _mapper.Map<UserResponseDto>(updatedUser);
        }
    }
}
