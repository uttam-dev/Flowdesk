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
    public class UpdateCategoryCommandTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<UpdateCategoryCommandHandler>> _loggerMock;
        private readonly UpdateCategoryCommandHandler _handler;

        public UpdateCategoryCommandTests()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<UpdateCategoryCommandHandler>>();

            _handler = new UpdateCategoryCommandHandler(
                _categoryRepositoryMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Should_ThrowBadRequestException_When_SLAHoursIsZeroOrNegative()
        {
            // Arrange
            var categoryId = 1;
            var dto = new UpdateCategoryDto
            {
                CategoryName = "Updated Name",
                SLAHours = 0,
                IsApprovalRequired = false
            };
            var command = new UpdateCategoryCommand(categoryId, dto);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("SLAHours must be greater than 0.");
        }

        [Fact]
        public async Task Should_ThrowNotFoundException_When_CategoryDoesNotExist()
        {
            // Arrange
            var categoryId = 1;
            var dto = new UpdateCategoryDto
            {
                CategoryName = "Updated Name",
                SLAHours = 24,
                IsApprovalRequired = false
            };
            var command = new UpdateCategoryCommand(categoryId, dto);

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync((Category?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Category not found");
        }

        [Fact]
        public async Task Should_UpdateCategory_When_ValidRequest()
        {
            // Arrange
            var categoryId = 1;
            var dto = new UpdateCategoryDto
            {
                CategoryName = "Updated Name",
                SLAHours = 36,
                IsApprovalRequired = true
            };
            var command = new UpdateCategoryCommand(categoryId, dto);
            var category = new Category
            {
                CategoryId = categoryId,
                CategoryName = "Old Name",
                SLAHours = 24,
                IsApprovalRequired = false
            };
            var expectedDto = new CategoryResponseDto
            {
                CategoryId = categoryId,
                CategoryName = dto.CategoryName,
                SLAHours = dto.SLAHours,
                IsApprovalRequired = (bool)dto.IsApprovalRequired
            };

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(category);
            _categoryRepositoryMock.Setup(repo => repo.Update(It.IsAny<Category>()))
                .ReturnsAsync((Category c) => c);
            _mapperMock.Setup(m => m.Map(dto, category))
                .Callback<UpdateCategoryDto, Category>((d, c) =>
                {
                    c.CategoryName = d.CategoryName!;
                    c.IsApprovalRequired = (bool)d.IsApprovalRequired!;
                });
            _mapperMock.Setup(m => m.Map<CategoryResponseDto>(category))
                .Returns(expectedDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            category.CategoryName.Should().Be(dto.CategoryName);
            category.SLAHours.Should().Be(dto.SLAHours);
            category.IsApprovalRequired.Should().BeTrue();
            category.UpdatedOn.Should().NotBeNull();
            result.Should().NotBeNull();
            result.CategoryName.Should().Be(dto.CategoryName);

            _categoryRepositoryMock.Verify(repo => repo.Update(category), Times.Once);
        }
    }
}
