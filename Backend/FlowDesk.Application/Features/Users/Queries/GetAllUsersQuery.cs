using FlowDesk.Application.Features.Users.DTOs;
using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;


namespace FlowDesk.Application.Features.Users.Queries
{
    public record GetAllUsersQuery(FilterUserDataQueryDto query): IRequest<PagedResult<UserResponseDto>>;
    
    public class GetAllUsersQueryHandler(IUserRepository userRepo) : IRequestHandler<GetAllUsersQuery, PagedResult<UserResponseDto>>
    {
        public async Task<PagedResult<UserResponseDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var (totalCount,users) = await userRepo.GetAllAsync(request.query);

            var mapped = users.Select(u => new UserResponseDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                RoleName = u.Role.RoleName,
                ManagerName = u.Manager?.FullName,
                IsActive = u.IsActive,
                CreatedOn = u.CreatedOn
            }).ToList();

            

            return new PagedResult<UserResponseDto>
            {
                Items = mapped,
                TotalCount = totalCount,
                PageNumber = request.query.PageNumber,
                PageSize = request.query.PageSize
            };
        }
    }

}
