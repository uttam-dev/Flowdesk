using FlowDesk.Application.Common.Exceptions;
using System.Security.Claims;

namespace FlowDesk.Application.Features.Users.Commands
{
    public record SoftDeleteUserCommand(int userId,int currentUserId):MediatR.IRequest;

    public class SoftDeleteUserCommandHandler(Domain.Interfaces.IUserRepository userRepository) : MediatR.IRequestHandler<SoftDeleteUserCommand>
    {
        public async Task Handle(SoftDeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(request.userId);
            if (user == null)
            {
                throw new Common.Exceptions.NotFoundException("User not found.");
            }

            if (user.UserId == request.currentUserId)
            {
                throw new BadRequestException("Invaldi request.");
            }
            user.IsDeleted = true;
            await userRepository.Update(user);
        }
    }

}
