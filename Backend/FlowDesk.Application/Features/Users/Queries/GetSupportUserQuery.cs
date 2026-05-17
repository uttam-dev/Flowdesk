using FlowDesk.Application.Features.Users.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Users.Queries
{
    public record GetSupportUserQuery : IRequest<List<SupportUserResponseDto>>;
    public class GetSupportUserQueryHandler(IUserRepository userRepo, ILogger<GetSupportUserQueryHandler> logger) : IRequestHandler<GetSupportUserQuery, List<SupportUserResponseDto>>
    {
        public async Task<List<SupportUserResponseDto>> Handle(GetSupportUserQuery request, CancellationToken cancellationToken)
        {
            // logging
            logger.LogInformation("Starting {Operation} with {@Request}", nameof(GetSupportUserQueryHandler), request);

            // logging
            logger.LogInformation("Fetching {Entity} list", "Manager");

            var supportUsers = await userRepo.GetSupportUsersAsync();

            // logging
            logger.LogInformation("Mapping {Entity} list to DTOs", "Manager");

            var result = supportUsers.Select(m => new SupportUserResponseDto
            {
                SupportUserId = m.UserId,
                FullName = m.FullName
            }).ToList();

            // logging
            logger.LogInformation("Successfully completed {Operation} for {Entity} list", nameof(GetSupportUserQueryHandler), "Manager");

            return result;
        }
    }
}
