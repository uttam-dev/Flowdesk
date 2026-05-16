using FlowDesk.Application.Features.Users.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Users.Commands
{
    public record UpdateUserDetailsCommand(int userId, UpdateUserDetailsDto userDto)
     : MediatR.IRequest<UserResponseDto>;

    public class UpdateUserDetailsCommandHandler(
            Domain.Interfaces.IUserRepository _userRepository,
            AutoMapper.IMapper _mapper)
        : MediatR.IRequestHandler<UpdateUserDetailsCommand, UserResponseDto>
    {

        public async Task<UserResponseDto> Handle(
            UpdateUserDetailsCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.userId);

            if (user == null)
            {
                throw new Common.Exceptions.NotFoundException("User not found.");
            }

            // Only updates non-null fields from DTO
            _mapper.Map(request.userDto, user);
            user.UpdatedOn = DateTime.UtcNow;

            var updatedUser = await _userRepository.Update(user);

            return _mapper.Map<UserResponseDto>(updatedUser);
        }
    }
}
