using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;

namespace FlowDesk.Application.Features.Users.Queries
{
    public record GetRolesQuery : IRequest<List<RoleResponseDto>>;
    public class GetRolesQueryHandler(IUnitOfWork _unitOfWork) : IRequestHandler<GetRolesQuery, List<RoleResponseDto>>
    {
        public async Task<List<RoleResponseDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
        {
            var roles = await _unitOfWork.GetAllRoles();
            return roles.Select(r => new RoleResponseDto
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName
            }).ToList();
        }
    }

}
