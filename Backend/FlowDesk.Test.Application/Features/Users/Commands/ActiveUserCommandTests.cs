using System;
using System.Threading;
using System.Threading.Tasks;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Features.Users.Commands;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FlowDesk.Application.Tests.Features.Users.Commands
{
    public class ActiveUserCommandTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ILogger<ActiveUserCommandHandler>> _loggerMock;
        private readonly ActiveUserCommandHandler _handler;

        public ActiveUserCommandTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _loggerMock = new Mock<ILogger<ActiveUserCommandHandler>>();

            _handler = new ActiveUserCommandHandler(
                _userRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Should_ThrowNotFoundException_When_UserDoesNotExist()
        {
            // Arrange
            var userId = 1;
            var command = new ActiveUserCommand(userId, 2);

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("User not found.");
        }

        [Fact]
        public async Task Should_ThrowBadRequestException_When_UserIsCurrentUser()
        {
            // Arrange
            var userId = 1;
            var command = new ActiveUserCommand(userId, userId); // Same user ID
            var user = new User { UserId = userId };

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("Invalid request.");
        }

        [Fact]
        public async Task Should_ActivateUser_When_ValidRequest()
        {
            // Arrange
            var userId = 1;
            var command = new ActiveUserCommand(userId, 2);
            var user = new User
            {
                UserId = userId,
                IsActive = false
            };

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            user.IsActive.Should().BeTrue();
            _userRepositoryMock.Verify(repo => repo.Update(user), Times.Once);
        }
    }
}
