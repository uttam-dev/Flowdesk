using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Users.Commands
{
    public record DeactiveUserCommand(int UserId,int CurentUser) : MediatR.IRequest;

    public class DeactiveUserCommandHandler(Domain.Interfaces.IUserRepository userRepository, ILogger<DeactiveUserCommandHandler> logger) : MediatR.IRequestHandler<DeactiveUserCommand>
    {
        public async Task Handle(DeactiveUserCommand request, CancellationToken cancellationToken)
        {
            // logging
            logger.LogInformation("Starting {Operation} with {@Request}", nameof(DeactiveUserCommandHandler), request);

            // logging
            logger.LogInformation("Fetching {Entity} with Id {EntityId}", "User", request.UserId);

            var user = await userRepository.GetByIdAsync(request.UserId);

            if (user == null)
            {
                // logging
                logger.LogWarning("{Entity} not found with Id {EntityId} in {Operation}", "User", request.UserId, nameof(DeactiveUserCommandHandler));
                throw new Common.Exceptions.NotFoundException("User not found.");
            }

            if (user.UserId == request.CurentUser)
            {
                // logging
                logger.LogWarning("Invalid operation in {Operation} for {Entity} with Id {EntityId}", nameof(DeactiveUserCommandHandler), "User", request.UserId);
                throw new Common.Exceptions.BadRequestException("Invalid request.");
            }

            user.IsActive = false;

            // logging
            logger.LogInformation("Updating {Entity} with Id {EntityId}", "User", request.UserId);

            await userRepository.Update(user);

            // logging
            logger.LogInformation("Successfully completed {Operation} for {Entity} with Id {EntityId}", nameof(DeactiveUserCommandHandler), "User", request.UserId);
        }
    }

}
