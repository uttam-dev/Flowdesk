using FlowDesk.Application.Features.Users.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Users.Commands
{
    public record UpdateUserDetailsCommand(int userId, UpdateUserDetailsDto userDto)
     : MediatR.IRequest<UserResponseDto>;

    public class UpdateUserDetailsCommandHandler
        : MediatR.IRequestHandler<UpdateUserDetailsCommand, UserResponseDto>
    {
        private readonly Domain.Interfaces.IUserRepository _userRepository;
        private readonly AutoMapper.IMapper _mapper;

        public UpdateUserDetailsCommandHandler(
            Domain.Interfaces.IUserRepository userRepository,
            AutoMapper.IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

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

            var updatedUser = await _userRepository.Update(user);

            return _mapper.Map<UserResponseDto>(updatedUser);
        }
    }
}
