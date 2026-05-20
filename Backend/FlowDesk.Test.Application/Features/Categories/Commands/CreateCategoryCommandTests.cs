using System;
using System.Threading;
using System.Threading.Tasks;
using FlowDesk.Application.Features.Categories.Commands;
using FlowDesk.Application.Features.Categories.DTOs;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FlowDesk.Application.Tests.Features.Categories.Commands
{
    public class CreateCategoryCommandTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<ILogger<CreateCategoryCommandHandler>> _loggerMock;
        private readonly CreateCategoryCommandHandler _handler;

        public CreateCategoryCommandTests()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _loggerMock = new Mock<ILogger<CreateCategoryCommandHandler>>();

            _handler = new CreateCategoryCommandHandler(
                _categoryRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Should_ThrowInvalidOperationException_When_SLAHoursIsZeroOrNegative()
        {
            // Arrange
            var dto = new CreateCategoryDto
            {
                CategoryName = "IT Support",
                SLAHours = 0,
                IsApprovalRequired = false
            };
            var command = new CreateCategoryCommand(dto);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("SLA hours must be greater than zero.");
        }

        [Fact]
        public async Task Should_ThrowInvalidOperationException_When_CategoryNameAlreadyExists()
        {
            // Arrange
            var dto = new CreateCategoryDto
            {
                CategoryName = "IT Support",
                SLAHours = 24,
                IsApprovalRequired = false
            };
            var command = new CreateCategoryCommand(dto);
            var existingCategory = new Category { CategoryId = 1, CategoryName = "IT Support" };

            _categoryRepositoryMock.Setup(repo => repo.GetByNameAsync(dto.CategoryName))
                .ReturnsAsync(existingCategory);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Category name already exists.");
        }

        [Fact]
        public async Task Should_CreateCategory_When_ValidRequest()
        {
            // Arrange
            var dto = new CreateCategoryDto
            {
                CategoryName = "New Category",
                SLAHours = 48,
                IsApprovalRequired = true
            };
            var command = new CreateCategoryCommand(dto);
            var createdCategory = new Category
            {
                CategoryId = 10,
                CategoryName = dto.CategoryName,
                SLAHours = dto.SLAHours,
                IsApprovalRequired = dto.IsApprovalRequired,
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };

            _categoryRepositoryMock.Setup(repo => repo.GetByNameAsync(dto.CategoryName))
                .ReturnsAsync((Category?)null);
            _categoryRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Category>()))
                .ReturnsAsync(createdCategory);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.CategoryId.Should().Be(10);
            result.CategoryName.Should().Be(dto.CategoryName);
            result.IsApprovalRequired.Should().BeTrue();
            result.IsActive.Should().BeTrue();

            _categoryRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Category>(c =>
                c.CategoryName == dto.CategoryName &&
                c.SLAHours == dto.SLAHours &&
                c.IsApprovalRequired == dto.IsApprovalRequired
            )), Times.Once);
        }
    }
}
