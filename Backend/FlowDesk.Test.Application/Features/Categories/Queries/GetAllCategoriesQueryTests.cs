using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlowDesk.Application.Features.Categories.Queries;
using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FlowDesk.Application.Tests.Features.Categories.Queries
{
    public class GetAllCategoriesQueryTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<ILogger<GetAllCategoriesQueryHandler>> _loggerMock;
        private readonly GetAllCategoriesQueryHandler _handler;

        public GetAllCategoriesQueryTests()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _loggerMock = new Mock<ILogger<GetAllCategoriesQueryHandler>>();

            _handler = new GetAllCategoriesQueryHandler(
                _categoryRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Should_ReturnPagedCategories_When_CategoriesExist()
        {
            // Arrange
            var queryDto = new FilterCategoryDataQueryDto
            {
                PageNumber = 1,
                PageSize = 10
            };
            var query = new GetAllCategoriesQuery(queryDto);
            var categories = new List<Category>
            {
                new Category { CategoryId = 1, CategoryName = "IT Support", IsActive = true, SLAHours = 24 },
                new Category { CategoryId = 2, CategoryName = "HR Support", IsActive = true, SLAHours = 48 }
            };

            _categoryRepositoryMock.Setup(repo => repo.GetAllAsync(queryDto))
                .ReturnsAsync((2, categories));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalCount.Should().Be(2);
            result.Items[0].CategoryName.Should().Be("IT Support");
            result.Items[1].CategoryName.Should().Be("HR Support");
        }
    }
}
