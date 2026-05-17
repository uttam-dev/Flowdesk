using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Users.Queries
{
    public record GetRolesQuery : IRequest<List<RoleResponseDto>>;
    public class GetRolesQueryHandler(IUnitOfWork _unitOfWork, ILogger<GetRolesQueryHandler> logger) : IRequestHandler<GetRolesQuery, List<RoleResponseDto>>
    {
        public async Task<List<RoleResponseDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
        {
            // logging
            logger.LogInformation("Starting {Operation} with {@Request}", nameof(GetRolesQueryHandler), request);

            // logging
            logger.LogInformation("Fetching {Entity} list", "Role");

            var roles = await _unitOfWork.GetAllRoles();

            // logging
            logger.LogInformation("Mapping {Entity} list to DTOs", "Role");

            var result = roles.Select(r => new RoleResponseDto
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName
            }).ToList();

            // logging
            logger.LogInformation("Successfully completed {Operation} for {Entity} list", nameof(GetRolesQueryHandler), "Role");

            return result;
        }
    }

}
