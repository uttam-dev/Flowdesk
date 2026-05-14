using AutoMapper;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Features.Users.DTOs;
using FlowDesk.Application.Services;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Users.Commands
{
    public record CreateUserCommand(CreateUserDto user) : IRequest<UserResponseDto>;

    public class CreateUserCommandHandler(IUserRepository userRepository,IMapper _mapper) : IRequestHandler<CreateUserCommand, UserResponseDto>
    {
        public async Task<UserResponseDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {

            bool isExist = await userRepository.EmailExistsAsync(request.user.Email);
            if(isExist)
            {
                throw new ConflictException("Email alredy exist.");
            }

            var user = _mapper.Map<User>(request.user);
            
            user.PasswordHash = PasswordService.HashPassword(request.user.Password);

            var userEntity = await userRepository.AddAsync(user);
            if(userEntity == null)
            {
                throw new InternalServerException("Failed to create user.");
            }

            return _mapper.Map<UserResponseDto>(userEntity);
        }
    }

}
