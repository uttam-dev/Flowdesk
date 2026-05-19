using FlowDesk.Application.Features.Dashboard.DTOs;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Interfaces;
using FlowDesk.Domain.Utils;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Dashboard.Queries
{
    public record GetDashboardQuery(int UserId, RoleEnum Role)
     : IRequest<DashboardResponseDto>;

    public class GetDashboardQueryHandler(
      IRequestRepository requestRepository,
      ILogger<GetDashboardQueryHandler> logger)
      : IRequestHandler<GetDashboardQuery, DashboardResponseDto>
    {
        public async Task<DashboardResponseDto> Handle(
            GetDashboardQuery request,
            CancellationToken cancellationToken)
        {
            logger.LogInformation(
                "Dashboard fetch started for UserId {UserId}, Role {Role}",
                request.UserId, request.Role);

            var data = await requestRepository.GetDashboardDataAsync(
                request.UserId,
                request.Role);

            var response = new DashboardResponseDto
            {
                TotalRequests = data.Total,
                Open = data.Open,
                PendingApproval = data.PendingApproval,
                Assigned = data.Assigned,
                InProgress = data.InProgress,
                Resolved = data.Resolved,
                Closed = data.Closed,
                RoleName = request.Role.ToString()
            };

            // ROLE BASED CUSTOMIZATION (clean & logical)
            switch (request.Role)
            {
                case RoleEnum.Employee:
                    response.Assigned = 0;
                    response.InProgress = 0;
                    break;

                case RoleEnum.Manager:
                    response.Assigned = 0;
                    break;

                case RoleEnum.Support:
                    response.PendingApproval = 0;
                    break;

                case RoleEnum.Admin:
                    // Admin sees everything
                    break;
            }

            // Chart AFTER role filtering
            response.StatusChart = new Dictionary<string, int>
        {
            { "Open", response.Open },
            { "PendingApproval", response.PendingApproval },
            { "Assigned", response.Assigned },
            { "InProgress", response.InProgress },
            { "Resolved", response.Resolved },
            { "Closed", response.Closed }
        };

            logger.LogInformation(
                "Dashboard fetch completed for UserId {UserId}",
                request.UserId);

            return response;
        }
    }
}
