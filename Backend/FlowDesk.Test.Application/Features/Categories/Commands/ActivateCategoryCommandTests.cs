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
    public class ActivateCategoryCommandTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<ActivateCategoryCommandHandler>> _loggerMock;
        private readonly ActivateCategoryCommandHandler _handler;

        public ActivateCategoryCommandTests()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<ActivateCategoryCommandHandler>>();

            _handler = new ActivateCategoryCommandHandler(
                _categoryRepositoryMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Should_ThrowNotFoundException_When_CategoryDoesNotExist()
        {
            // Arrange
            var categoryId = 1;
            var command = new ActivateCategoryCommand(categoryId);

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync((Category?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Category not found");
        }

        [Fact]
        public async Task Should_ActivateCategory_When_CategoryExists()
        {
            // Arrange
            var categoryId = 1;
            var command = new ActivateCategoryCommand(categoryId);
            var category = new Category
            {
                CategoryId = categoryId,
                CategoryName = "IT Support",
                IsActive = false
            };
            var expectedDto = new CategoryResponseDto
            {
                CategoryId = categoryId,
                CategoryName = "IT Support",
                IsActive = true
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
            category.IsActive.Should().BeTrue();
            category.UpdatedOn.Should().NotBeNull();
            result.Should().NotBeNull();
            result.IsActive.Should().BeTrue();

            _categoryRepositoryMock.Verify(repo => repo.Update(category), Times.Once);
        }
    }
}
