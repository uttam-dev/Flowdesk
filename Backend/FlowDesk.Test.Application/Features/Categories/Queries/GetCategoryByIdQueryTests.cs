using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FlowDesk.Application.Features.Categories.Queries;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FlowDesk.Application.Tests.Features.Categories.Queries
{
    public class GetCategoryByIdQueryTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<ILogger<GetCategoryByIdQueryHandler>> _loggerMock;
        private readonly GetCategoryByIdQueryHandler _handler;

        public GetCategoryByIdQueryTests()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _loggerMock = new Mock<ILogger<GetCategoryByIdQueryHandler>>();

            _handler = new GetCategoryByIdQueryHandler(
                _categoryRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Should_ThrowKeyNotFoundException_When_CategoryDoesNotExist()
        {
            // Arrange
            var categoryId = 1;
            var query = new GetCategoryByIdQuery(categoryId);

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync((Category?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Category not found.");
        }

        [Fact]
        public async Task Should_ReturnCategoryDto_When_CategoryExists()
        {
            // Arrange
            var categoryId = 1;
            var query = new GetCategoryByIdQuery(categoryId);
            var category = new Category
            {
                CategoryId = categoryId,
                CategoryName = "IT Support",
                IsActive = true,
                SLAHours = 24,
                CreatedOn = DateTime.UtcNow
            };

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.CategoryId.Should().Be(categoryId);
            result.CategoryName.Should().Be("IT Support");
            result.IsActive.Should().BeTrue();
            result.SLAHours.Should().Be(24);
        }
    }
}
