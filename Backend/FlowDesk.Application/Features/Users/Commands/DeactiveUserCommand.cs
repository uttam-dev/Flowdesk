using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Users.Commands
{
    public record DeactiveUserCommand(int UserId,int CurentUser) : MediatR.IRequest; 

    public class DeactiveUserCommandHandler(Domain.Interfaces.IUserRepository userRepository) : MediatR.IRequestHandler<DeactiveUserCommand>
    {
        public async Task Handle(DeactiveUserCommand request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                throw new Common.Exceptions.NotFoundException("User not found.");
            }
            if (user.UserId == request.CurentUser)
            {
                throw new Common.Exceptions.BadRequestException("Invalid request.");
            }
            user.IsActive = false;
            await userRepository.Update(user);
        }
    }

}
