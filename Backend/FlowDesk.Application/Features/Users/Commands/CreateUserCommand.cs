using AutoMapper;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Features.Users.DTOs;
using FlowDesk.Application.Services;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Interfaces;
using FlowDesk.Domain.Utils;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Users.Commands
{
    public record CreateUserCommand(CreateUserDto user) : IRequest<UserResponseDto>;

    public class CreateUserCommandHandler(IUserRepository userRepository, IMapper _mapper, ILogger<CreateUserCommandHandler> logger) : IRequestHandler<CreateUserCommand, UserResponseDto>
    {
        public async Task<UserResponseDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            // logging
            logger.LogInformation("Starting {Operation} with {@Request}", nameof(CreateUserCommandHandler), request);

            // logging
            logger.LogInformation("Checking existing {Entity} with Email {Entity}", "User", request.user.Email);

            bool isExist = await userRepository.EmailExistsAsync(request.user.Email);

            if (isExist)
            {
                // logging
                logger.LogWarning("{Entity} already exists with Email {Entity} in {Operation}", "User", request.user.Email, nameof(CreateUserCommandHandler));
                throw new ConflictException("Email alredy exist.");
            }

            if (request.user.RoleId == (int)RoleEnum.Employee && request.user.ManagerId == null)
            {
                throw new BadRequestException("Employee role must contain manager");
            }
            // logging
            logger.LogInformation("Mapping request to {Entity}", "User");

            var user = _mapper.Map<User>(request.user);

            user.PasswordHash = PasswordService.HashPassword(request.user.Password);

            // logging
            logger.LogInformation("Saving {Entity} to repository", "User");

            var userEntity = await userRepository.AddAsync(user);

            if (userEntity == null)
            {
                // logging
                logger.LogWarning("Failed to create {Entity} in {Operation}", "User", nameof(CreateUserCommandHandler));
                throw new InternalServerException("Failed to create user.");
            }

            // logging
            logger.LogInformation("Successfully created {Entity} with Id {EntityId} in {Operation}", "User", userEntity.UserId, nameof(CreateUserCommandHandler));

            return _mapper.Map<UserResponseDto>(userEntity);
        }
    }

}
