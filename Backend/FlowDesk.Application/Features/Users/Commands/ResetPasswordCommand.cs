using FlowDesk.Application.Services;

namespace FlowDesk.Application.Features.Users.Commands
{
    public record ResetPasswordCommand(int UserId, DTOs.UserResetPasswordDto ResetPasswordDto) : MediatR.IRequest;

    public class ResetPasswordCommandHandler(Domain.Interfaces.IUserRepository userRepository) : MediatR.IRequestHandler<ResetPasswordCommand>
    {
        public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                throw new Common.Exceptions.NotFoundException("User not found.");
            }
            user.PasswordHash = PasswordService.HashPassword(request.ResetPasswordDto.NewPassword);
            await userRepository.Update(user);
        }
    }
}