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
    public class DeactiveUserCommandTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ILogger<DeactiveUserCommandHandler>> _loggerMock;
        private readonly DeactiveUserCommandHandler _handler;

        public DeactiveUserCommandTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _loggerMock = new Mock<ILogger<DeactiveUserCommandHandler>>();

            _handler = new DeactiveUserCommandHandler(
                _userRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Should_ThrowNotFoundException_When_UserDoesNotExist()
        {
            // Arrange
            var userId = 1;
            var command = new DeactiveUserCommand(userId, 2);

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
            var command = new DeactiveUserCommand(userId, userId); // Same user ID
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
        public async Task Should_DeactivateUser_When_ValidRequest()
        {
            // Arrange
            var userId = 1;
            var command = new DeactiveUserCommand(userId, 2);
            var user = new User
            {
                UserId = userId,
                IsActive = true
            };

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            user.IsActive.Should().BeFalse();
            _userRepositoryMock.Verify(repo => repo.Update(user), Times.Once);
        }
    }
}
