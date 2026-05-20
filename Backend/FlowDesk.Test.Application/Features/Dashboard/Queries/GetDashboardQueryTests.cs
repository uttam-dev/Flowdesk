using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FlowDesk.Application.Features.Dashboard.Queries;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FlowDesk.Application.Tests.Features.Dashboard.Queries
{
    public class GetDashboardQueryTests
    {
        private readonly Mock<IRequestRepository> _requestRepositoryMock;
        private readonly Mock<ILogger<GetDashboardQueryHandler>> _loggerMock;
        private readonly GetDashboardQueryHandler _handler;

        public GetDashboardQueryTests()
        {
            _requestRepositoryMock = new Mock<IRequestRepository>();
            _loggerMock = new Mock<ILogger<GetDashboardQueryHandler>>();

            _handler = new GetDashboardQueryHandler(
                _requestRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Theory]
        [InlineData(RoleEnum.Employee, 0, 0, 10, 5)] // Employee should have Assigned = 0, InProgress = 0
        [InlineData(RoleEnum.Manager, 0, 8, 10, 5)]  // Manager should have Assigned = 0
        [InlineData(RoleEnum.Support, 4, 8, 0, 5)]  // Support should have PendingApproval = 0
        [InlineData(RoleEnum.Admin, 4, 8, 10, 5)]    // Admin keeps all values
        public async Task Should_ApplyRoleBasedFilters_When_FetchingDashboard(
            RoleEnum role,
            int expectedAssigned,
            int expectedInProgress,
            int expectedPendingApproval,
            int expectedOpen)
        {
            // Arrange
            var userId = 1;
            var query = new GetDashboardQuery(userId, role);

            // Mock dashboard data: (Total, Open, PendingApproval, Assigned, InProgress, Resolved, Closed)
            _requestRepositoryMock.Setup(repo => repo.GetDashboardDataAsync(userId, role))
                .ReturnsAsync((30, 5, 10, 4, 8, 2, 1));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.RoleName.Should().Be(role.ToString());
            result.Open.Should().Be(expectedOpen);
            result.Assigned.Should().Be(expectedAssigned);
            result.InProgress.Should().Be(expectedInProgress);
            result.PendingApproval.Should().Be(expectedPendingApproval);

            // Verify chart updates match the response properties
            result.StatusChart.Should().ContainKey("Assigned").WhoseValue.Should().Be(expectedAssigned);
            result.StatusChart.Should().ContainKey("InProgress").WhoseValue.Should().Be(expectedInProgress);
            result.StatusChart.Should().ContainKey("PendingApproval").WhoseValue.Should().Be(expectedPendingApproval);
        }
    }
}
