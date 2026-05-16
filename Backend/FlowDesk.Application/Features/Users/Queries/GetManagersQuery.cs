using FlowDesk.Application.Features.Users.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;

namespace FlowDesk.Application.Features.Users.Queries
{
    public record GetManagersQuery : IRequest<List<ManagerResponseDto>>;
    public class GetManagersQueryHandler(IUserRepository userRepo) : IRequestHandler<GetManagersQuery, List<ManagerResponseDto>>
    {
        public async Task<List<ManagerResponseDto>> Handle(GetManagersQuery request, CancellationToken cancellationToken)
        {
            var managers = await userRepo.GetManagersAsync();

            return managers.Select(m => new ManagerResponseDto
            {
                ManagerId = m.UserId,
                FullName = m.FullName
            }).ToList();
        }
    }
}
