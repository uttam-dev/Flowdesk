using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FlowDesk.Application.Common.Exceptions;
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
    public class DeactivateCategoryCommandTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<DeactivateCategoryCommandHandler>> _loggerMock;
        private readonly DeactivateCategoryCommandHandler _handler;

        public DeactivateCategoryCommandTests()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<DeactivateCategoryCommandHandler>>();

            _handler = new DeactivateCategoryCommandHandler(
                _categoryRepositoryMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Should_ThrowNotFoundException_When_CategoryDoesNotExist()
        {
            // Arrange
            var categoryId = 1;
            var command = new DeactivateCategoryCommand(categoryId);

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync((Category?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Category not found");
        }

        [Fact]
        public async Task Should_DeactivateCategory_When_CategoryExists()
        {
            // Arrange
            var categoryId = 1;
            var command = new DeactivateCategoryCommand(categoryId);
            var category = new Category
            {
                CategoryId = categoryId,
                CategoryName = "IT Support",
                IsActive = true
            };
            var expectedDto = new CategoryResponseDto
            {
                CategoryId = categoryId,
                CategoryName = "IT Support",
                IsActive = false
            };

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(category);
            _categoryRepositoryMock.Setup(repo => repo.Update(It.IsAny<Category>()))
                .ReturnsAsync((Category c) => c);
            _mapperMock.Setup(m => m.Map<CategoryResponseDto>(It.IsAny<Category>()))
                .Returns(expectedDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            category.IsActive.Should().BeFalse();
            category.UpdatedOn.Should().NotBeNull();
            result.Should().NotBeNull();
            result.IsActive.Should().BeFalse();

            _categoryRepositoryMock.Verify(repo => repo.Update(category), Times.Once);
        }
    }
}
