using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Users.Commands
{
    public record SoftDeleteUserCommand(int userId):MediatR.IRequest;

    public class SoftDeleteUserCommandHandler(Domain.Interfaces.IUserRepository userRepository) : MediatR.IRequestHandler<SoftDeleteUserCommand>
    {
        public async Task Handle(SoftDeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(request.userId);
            if (user == null)
            {
                throw new Common.Exceptions.NotFoundException("User not found.");
            }
            user.IsDeleted = true;
            await userRepository.Update(user);
        }
    }

}
