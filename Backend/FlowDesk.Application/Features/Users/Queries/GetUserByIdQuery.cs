using MediatR;
using FlowDesk.Application.Features.Users.DTOs;
using FlowDesk.Domain.Interfaces;
using AutoMapper;
using FlowDesk.Application.Common.Exceptions;

namespace FlowDesk.Application.Features.Users.Queries
{
    public record GetUserByIdQuery(int UserId) : IRequest<UserResponseDto>;
    
    public class GetUserByIdQueryHandler(IUserRepository userRepository, IMapper _mapper) : IRequestHandler<GetUserByIdQuery, UserResponseDto>
    {
        public async Task<UserResponseDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(request.UserId);
            if(user == null)
            {
                throw new NotFoundException("User not found.");
            }
            return _mapper.Map<UserResponseDto>(user);
        }
    }
}
