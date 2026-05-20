using System.Threading;
using System.Threading.Tasks;
using FlowDesk.Application.Features.Dashboard.Queries;
using FlowDesk.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FlowDesk.Application.Tests.Features.Dashboard.Queries
{
    public class GetSlaSummaryQueryTests
    {
        private readonly Mock<IRequestRepository> _requestRepositoryMock;
        private readonly Mock<ILogger<GetSlaSummaryQueryHandler>> _loggerMock;
        private readonly GetSlaSummaryQueryHandler _handler;

        public GetSlaSummaryQueryTests()
        {
            _requestRepositoryMock = new Mock<IRequestRepository>();
            _loggerMock = new Mock<ILogger<GetSlaSummaryQueryHandler>>();

            _handler = new GetSlaSummaryQueryHandler(
                _requestRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Should_ReturnSlaSummaryDto_When_QueryIsExecuted()
        {
            // Arrange
            var query = new GetSlaSummaryQuery();
            // Mock returns tuple: (WithinSla, NearingBreach, Breached, Escalated)
            _requestRepositoryMock.Setup(repo => repo.GetSlaSummaryAsync())
                .ReturnsAsync((10, 5, 2, 3));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.WithinSla.Should().Be(10);
            result.NearingBreach.Should().Be(5);
            result.Breached.Should().Be(2);
            result.Escalated.Should().Be(3);

            _requestRepositoryMock.Verify(repo => repo.GetSlaSummaryAsync(), Times.Once);
        }
    }
}
