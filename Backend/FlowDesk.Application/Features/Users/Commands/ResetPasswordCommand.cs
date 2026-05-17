using FlowDesk.Application.Services;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Users.Commands
{
    public record ResetPasswordCommand(int UserId, DTOs.UserResetPasswordDto ResetPasswordDto) : MediatR.IRequest;

    public class ResetPasswordCommandHandler(Domain.Interfaces.IUserRepository userRepository, ILogger<ResetPasswordCommandHandler> logger) : MediatR.IRequestHandler<ResetPasswordCommand>
    {
        public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            // logging
            logger.LogInformation("Starting {Operation} with {@Request}", nameof(ResetPasswordCommandHandler), request);

            // logging
            logger.LogInformation("Fetching {Entity} with Id {EntityId}", "User", request.UserId);

            var user = await userRepository.GetByIdAsync(request.UserId);

            if (user == null)
            {
                // logging
                logger.LogWarning("{Entity} not found with Id {EntityId} in {Operation}", "User", request.UserId, nameof(ResetPasswordCommandHandler));
                throw new Common.Exceptions.NotFoundException("User not found.");
            }

            // logging
            logger.LogInformation("Updating password for {Entity} with Id {EntityId}", "User", request.UserId);

            user.PasswordHash = PasswordService.HashPassword(request.ResetPasswordDto.NewPassword);

            user.UpdatedOn = DateTime.UtcNow;

            // logging
            logger.LogInformation("Saving updated {Entity} with Id {EntityId}", "User", request.UserId);

            await userRepository.Update(user);

            // logging
            logger.LogInformation("Successfully completed {Operation} for {Entity} with Id {EntityId}", nameof(ResetPasswordCommandHandler), "User", request.UserId);
        }
    }
}