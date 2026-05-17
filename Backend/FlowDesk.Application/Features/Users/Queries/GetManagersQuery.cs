using FlowDesk.Application.Features.Users.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Users.Queries
{
    public record GetManagersQuery : IRequest<List<ManagerResponseDto>>;
    public class GetManagersQueryHandler(IUserRepository userRepo, ILogger<GetManagersQueryHandler> logger) : IRequestHandler<GetManagersQuery, List<ManagerResponseDto>>
    {
        public async Task<List<ManagerResponseDto>> Handle(GetManagersQuery request, CancellationToken cancellationToken)
        {
            // logging
            logger.LogInformation("Starting {Operation} with {@Request}", nameof(GetManagersQueryHandler), request);

            // logging
            logger.LogInformation("Fetching {Entity} list", "Manager");

            var managers = await userRepo.GetManagersAsync();

            // logging
            logger.LogInformation("Mapping {Entity} list to DTOs", "Manager");

            var result = managers.Select(m => new ManagerResponseDto
            {
                ManagerId = m.UserId,
                FullName = m.FullName
            }).ToList();

            // logging
            logger.LogInformation("Successfully completed {Operation} for {Entity} list", nameof(GetManagersQueryHandler), "Manager");

            return result;
        }
    }
}
