using FlowDesk.Application.Features.Users.DTOs;
using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;


namespace FlowDesk.Application.Features.Users.Queries
{
    public record GetAllUsersQuery(FilterUserDataQueryDto query): IRequest<PagedResult<UserResponseDto>>;

    public class GetAllUsersQueryHandler(IUserRepository userRepo, ILogger<GetAllUsersQueryHandler> logger) : IRequestHandler<GetAllUsersQuery, PagedResult<UserResponseDto>>
    {
        public async Task<PagedResult<UserResponseDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            // logging
            logger.LogInformation("Starting {Operation} with {@Request}", nameof(GetAllUsersQueryHandler), request);

            // logging
            logger.LogInformation("Fetching {Entity} list with pagination {@Request}", "User", request.query);

            var (totalCount, users) = await userRepo.GetAllAsync(request.query);

            // logging
            logger.LogInformation("Mapping {Entity} list to DTOs", "User");

            var mapped = users.Select(u => new UserResponseDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                RoleName = u.Role.RoleName,
                ManagerName = u.Manager?.FullName,
                IsActive = u.IsActive,
                CreatedOn = u.CreatedOn,
                UpdatedOn = u.UpdatedOn
            }).ToList();

            // logging
            logger.LogInformation("Successfully completed {Operation} for {Entity} list", nameof(GetAllUsersQueryHandler), "User");

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
