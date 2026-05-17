using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Users.Commands
{
    public record ActiveUserCommand(int userId,int curentUser) : MediatR.IRequest;

    public class ActiveUserCommandHandler(Domain.Interfaces.IUserRepository userRepository, ILogger<ActiveUserCommandHandler> logger) : MediatR.IRequestHandler<ActiveUserCommand>
    {
        public async Task Handle(ActiveUserCommand request, CancellationToken cancellationToken)
        {
            // logging
            logger.LogInformation("Starting {Operation} with {@Request}", nameof(ActiveUserCommandHandler), request);

            // logging
            logger.LogInformation("Fetching {Entity} with Id {EntityId}", "User", request.userId);

            var user = await userRepository.GetByIdAsync(request.userId);

            if (user == null)
            {
                // logging
                logger.LogWarning("{Entity} not found with Id {EntityId} in {Operation}", "User", request.userId, nameof(ActiveUserCommandHandler));
                throw new Common.Exceptions.NotFoundException("User not found.");
            }

            if (user.UserId == request.curentUser)
            {
                // logging
                logger.LogWarning("Invalid operation in {Operation} for {Entity} with Id {EntityId}", nameof(ActiveUserCommandHandler), "User", request.userId);
                throw new Common.Exceptions.BadRequestException("Invalid request.");
            }

            user.IsActive = true;

            // logging
            logger.LogInformation("Updating {Entity} with Id {EntityId}", "User", request.userId);

            await userRepository.Update(user);

            // logging
            logger.LogInformation("Successfully completed {Operation} for {Entity} with Id {EntityId}", nameof(ActiveUserCommandHandler), "User", request.userId);
        }
    }
}
