using System;
using System.Threading;
using System.Threading.Tasks;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Features.Requests.Commands;
using FlowDesk.Application.Features.Requests.DTOs;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FlowDesk.Application.Tests.Features.Requests.Commands
{
    public class UpdateRequestStatusCommandTests
    {
        private readonly Mock<IRequestRepository> _requestRepositoryMock;
        private readonly Mock<ICommentRepository> _commentRepositoryMock;
        private readonly Mock<IRequestHistoryRepository> _historyRepositoryMock;
        private readonly Mock<ILogger<UpdateRequestStatusCommandHandler>> _loggerMock;
        private readonly UpdateRequestStatusCommandHandler _handler;

        public UpdateRequestStatusCommandTests()
        {
            _requestRepositoryMock = new Mock<IRequestRepository>();
            _commentRepositoryMock = new Mock<ICommentRepository>();
            _historyRepositoryMock = new Mock<IRequestHistoryRepository>();
            _loggerMock = new Mock<ILogger<UpdateRequestStatusCommandHandler>>();

            _handler = new UpdateRequestStatusCommandHandler(
                _requestRepositoryMock.Object,
                _commentRepositoryMock.Object,
                _historyRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Should_ThrowNotFoundException_When_RequestDoesNotExist()
        {
            // Arrange
            var requestId = 1;
            var command = new UpdateRequestStatusCommand(
                requestId,
                10,
                RoleEnum.Support.ToString(),
                new UpdateRequestStatusDto { Status = RequestStatusEnum.InProgress });

            _requestRepositoryMock.Setup(repo => repo.GetByIdAsync(requestId))
                .ReturnsAsync((Request?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Request not found.");
        }

        [Fact]
        public async Task Should_ThrowUnauthorizedException_When_UserIsNotSupportRole()
        {
            // Arrange
            var requestId = 1;
            var request = new Request
            {
                RequestId = requestId,
                AssignedToId = 10,
                Status = RequestStatusEnum.Assigned
            };
            var command = new UpdateRequestStatusCommand(
                requestId,
                10,
                RoleEnum.Employee.ToString(), // Not Support
                new UpdateRequestStatusDto { Status = RequestStatusEnum.InProgress });

            _requestRepositoryMock.Setup(repo => repo.GetByIdAsync(requestId))
                .ReturnsAsync(request);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedException>()
                .WithMessage("Only support can update status.");
        }

        [Fact]
        public async Task Should_ThrowUnauthorizedException_When_RequestIsNotAssignedToUser()
        {
            // Arrange
            var requestId = 1;
            var request = new Request
            {
                RequestId = requestId,
                AssignedToId = 10, // Assigned to user 10
                Status = RequestStatusEnum.Assigned
            };
            var command = new UpdateRequestStatusCommand(
                requestId,
                20, // Different user
                RoleEnum.Support.ToString(),
                new UpdateRequestStatusDto { Status = RequestStatusEnum.InProgress });

            _requestRepositoryMock.Setup(repo => repo.GetByIdAsync(requestId))
                .ReturnsAsync(request);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedException>()
                .WithMessage("You are not assigned to this request.");
        }

        [Fact]
        public async Task Should_ThrowBadRequestException_When_NewStatusIsInvalid()
        {
            // Arrange
            var requestId = 1;
            var userId = 10;
            var request = new Request
            {
                RequestId = requestId,
                AssignedToId = userId,
                Status = RequestStatusEnum.Assigned
            };
            var command = new UpdateRequestStatusCommand(
                requestId,
                userId,
                RoleEnum.Support.ToString(),
                new UpdateRequestStatusDto { Status = RequestStatusEnum.Open }); // Invalid target status for status update

            _requestRepositoryMock.Setup(repo => repo.GetByIdAsync(requestId))
                .ReturnsAsync(request);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("Invalid status.");
        }

        [Fact]
        public async Task Should_ThrowBadRequestException_When_TransitionToInProgressIsInvalid()
        {
            // Arrange
            var requestId = 1;
            var userId = 10;
            var request = new Request
            {
                RequestId = requestId,
                AssignedToId = userId,
                Status = RequestStatusEnum.Open // Not Assigned
            };
            var command = new UpdateRequestStatusCommand(
                requestId,
                userId,
                RoleEnum.Support.ToString(),
                new UpdateRequestStatusDto { Status = RequestStatusEnum.InProgress });

            _requestRepositoryMock.Setup(repo => repo.GetByIdAsync(requestId))
                .ReturnsAsync(request);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("Only assigned request can move to InProgress.");
        }

        [Fact]
        public async Task Should_ThrowBadRequestException_When_TransitionToResolvedIsInvalid()
        {
            // Arrange
            var requestId = 1;
            var userId = 10;
            var request = new Request
            {
                RequestId = requestId,
                AssignedToId = userId,
                Status = RequestStatusEnum.Assigned // Not InProgress
            };
            var command = new UpdateRequestStatusCommand(
                requestId,
                userId,
                RoleEnum.Support.ToString(),
                new UpdateRequestStatusDto { Status = RequestStatusEnum.Resolved });

            _requestRepositoryMock.Setup(repo => repo.GetByIdAsync(requestId))
                .ReturnsAsync(request);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("Only InProgress request can be resolved.");
        }

        [Fact]
        public async Task Should_UpdateStatusToInProgress_When_TransitionFromAssignedIsSuccess()
        {
            // Arrange
            var requestId = 1;
            var userId = 10;
            var request = new Request
            {
                RequestId = requestId,
                AssignedToId = userId,
                Status = RequestStatusEnum.Assigned
            };
            var dto = new UpdateRequestStatusDto
            {
                Status = RequestStatusEnum.InProgress,
                RemarksId = 5,
                CommentText = null
            };
            var command = new UpdateRequestStatusCommand(
                requestId,
                userId,
                RoleEnum.Support.ToString(),
                dto);

            _requestRepositoryMock.Setup(repo => repo.GetByIdAsync(requestId))
                .ReturnsAsync(request);
            _requestRepositoryMock.Setup(repo => repo.Update(It.IsAny<Request>()))
                .ReturnsAsync((Request r) => r);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            request.Status.Should().Be(RequestStatusEnum.InProgress);
            _requestRepositoryMock.Verify(repo => repo.Update(request), Times.Once);
            _historyRepositoryMock.Verify(history => history.AddAsync(It.Is<RequestHistory>(h =>
                h.RequestId == requestId &&
                h.ChangedById == userId &&
                h.OldStatus == RequestStatusEnum.Assigned &&
                h.NewStatus == RequestStatusEnum.InProgress &&
                h.RemarksId == dto.RemarksId
            )), Times.Once);
        }

        [Fact]
        public async Task Should_AddComment_When_CommentTextIsProvided()
        {
            // Arrange
            var requestId = 1;
            var userId = 10;
            var request = new Request
            {
                RequestId = requestId,
                AssignedToId = userId,
                Status = RequestStatusEnum.Assigned
            };
            var dto = new UpdateRequestStatusDto
            {
                Status = RequestStatusEnum.InProgress,
                RemarksId = null,
                CommentText = "Status updated to In Progress"
            };
            var command = new UpdateRequestStatusCommand(
                requestId,
                userId,
                RoleEnum.Support.ToString(),
                dto);

            _requestRepositoryMock.Setup(repo => repo.GetByIdAsync(requestId))
                .ReturnsAsync(request);
            _requestRepositoryMock.Setup(repo => repo.Update(It.IsAny<Request>()))
                .ReturnsAsync((Request r) => r);
            _commentRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Comment>()))
                .ReturnsAsync((Comment c) => c);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _commentRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Comment>(c =>
                c.RequestId == requestId &&
                c.CommentById == userId &&
                c.CommentText == dto.CommentText
            )), Times.Once);
        }

        [Fact]
        public async Task Should_NotAddComment_When_CommentTextIsEmptyOrWhitespace()
        {
            // Arrange
            var requestId = 1;
            var userId = 10;
            var request = new Request
            {
                RequestId = requestId,
                AssignedToId = userId,
                Status = RequestStatusEnum.Assigned
            };
            var dto = new UpdateRequestStatusDto
            {
                Status = RequestStatusEnum.InProgress,
                RemarksId = null,
                CommentText = "   "
            };
            var command = new UpdateRequestStatusCommand(
                requestId,
                userId,
                RoleEnum.Support.ToString(),
                dto);

            _requestRepositoryMock.Setup(repo => repo.GetByIdAsync(requestId))
                .ReturnsAsync(request);
            _requestRepositoryMock.Setup(repo => repo.Update(It.IsAny<Request>()))
                .ReturnsAsync((Request r) => r);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _commentRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Comment>()), Times.Never);
        }

        [Fact]
        public async Task Should_UpdateStatusToResolvedAndAutoClose_When_TransitionFromInProgressIsSuccess()
        {
            // Arrange
            var requestId = 1;
            var userId = 10;
            var request = new Request
            {
                RequestId = requestId,
                AssignedToId = userId,
                Status = RequestStatusEnum.InProgress
            };
            var dto = new UpdateRequestStatusDto
            {
                Status = RequestStatusEnum.Resolved,
                RemarksId = 3,
                CommentText = null
            };
            var command = new UpdateRequestStatusCommand(
                requestId,
                userId,
                RoleEnum.Support.ToString(),
                dto);

            _requestRepositoryMock.Setup(repo => repo.GetByIdAsync(requestId))
                .ReturnsAsync(request);
            
            var statusesUpdated = new System.Collections.Generic.List<RequestStatusEnum>();
            _requestRepositoryMock.Setup(repo => repo.Update(It.IsAny<Request>()))
                .Callback<Request>(r => statusesUpdated.Add(r.Status))
                .ReturnsAsync((Request r) => r);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            request.Status.Should().Be(RequestStatusEnum.Closed);
            request.ClosedOn.Should().NotBeNull();
            request.ClosedOn.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

            // Verify both status updates occurred
            statusesUpdated.Should().ContainInOrder(RequestStatusEnum.Resolved, RequestStatusEnum.Closed);

            // Verify both history entries: resolved history and system-generated closed history
            _historyRepositoryMock.Verify(history => history.AddAsync(It.Is<RequestHistory>(h =>
                h.RequestId == requestId &&
                h.ChangedById == userId &&
                h.OldStatus == RequestStatusEnum.InProgress &&
                h.NewStatus == RequestStatusEnum.Resolved &&
                h.RemarksId == dto.RemarksId
            )), Times.Once);

            _historyRepositoryMock.Verify(history => history.AddAsync(It.Is<RequestHistory>(h =>
                h.RequestId == requestId &&
                h.OldStatus == RequestStatusEnum.Resolved &&
                h.NewStatus == RequestStatusEnum.Closed &&
                h.IsSystemGenerated == true
            )), Times.Once);
        }
    }
}
