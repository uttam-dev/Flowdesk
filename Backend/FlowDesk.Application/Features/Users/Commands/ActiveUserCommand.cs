using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Users.Commands
{
    public record ActiveUserCommand(int userId,int curentUser) : MediatR.IRequest; 

    public class ActiveUserCommandHandler(Domain.Interfaces.IUserRepository userRepository) : MediatR.IRequestHandler<ActiveUserCommand>
    {
        public async Task Handle(ActiveUserCommand request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(request.userId);
            if (user == null)
            {
                throw new Common.Exceptions.NotFoundException("User not found.");
            }
            if (user.UserId == request.curentUser)
            {
                throw new Common.Exceptions.BadRequestException("Invalid request.");
            }

            user.IsActive = true;
            await userRepository.Update(user);
        }
    }
}
