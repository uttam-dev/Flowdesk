using FlowDesk.Application.Common.Exceptions;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace FlowDesk.Application.Features.Users.Commands
{
    public record SoftDeleteUserCommand(int userId,int currentUserId):MediatR.IRequest;

    public class SoftDeleteUserCommandHandler(Domain.Interfaces.IUserRepository userRepository, ILogger<SoftDeleteUserCommandHandler> logger) : MediatR.IRequestHandler<SoftDeleteUserCommand>
    {
        public async Task Handle(SoftDeleteUserCommand request, CancellationToken cancellationToken)
        {
            // logging
            logger.LogInformation("Starting {Operation} with {@Request}", nameof(SoftDeleteUserCommandHandler), request);

            // logging
            logger.LogInformation("Fetching {Entity} with Id {EntityId}", "User", request.userId);

            var user = await userRepository.GetByIdAsync(request.userId);

            if (user == null)
            {
                // logging
                logger.LogWarning("{Entity} not found with Id {EntityId} in {Operation}", "User", request.userId, nameof(SoftDeleteUserCommandHandler));
                throw new Common.Exceptions.NotFoundException("User not found.");
            }

            if (user.UserId == request.currentUserId)
            {
                // logging
                logger.LogWarning("Invalid operation in {Operation} for {Entity} with Id {EntityId}", nameof(SoftDeleteUserCommandHandler), "User", request.userId);
                throw new BadRequestException("Invaldi request.");
            }

            user.IsDeleted = true;

            // logging
            logger.LogInformation("Updating {Entity} with Id {EntityId}", "User", request.userId);

            await userRepository.Update(user);

            // logging
            logger.LogInformation("Successfully completed {Operation} for {Entity} with Id {EntityId}", nameof(SoftDeleteUserCommandHandler), "User", request.userId);
        }
    }

}
