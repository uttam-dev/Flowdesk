using FlowDesk.Application.Features.Categories.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Categories.Commands
{
    public record CreateCategoryCommand(CreateCategoryDto Dto)
    : IRequest<CategoryResponseDto>;

    public class CreateCategoryCommandHandler(ICategoryRepository categoryRepository, ILogger<CreateCategoryCommandHandler> logger) : IRequestHandler<CreateCategoryCommand, CategoryResponseDto>
    {
        public async Task<CategoryResponseDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            // logging
            logger.LogInformation("Starting {Operation} with {@Request}", nameof(CreateCategoryCommandHandler), request);

            //loggin
            if(request.Dto.SLAHours <= 0)
            {
                logger.LogWarning("Invalid SLAHours {SLAHours} in {Operation}", request.Dto.SLAHours, nameof(CreateCategoryCommandHandler));
                throw new InvalidOperationException("SLA hours must be greater than zero.");
            }

            // logging
            logger.LogInformation("Checking existing {Entity} with name {Entity}", "Category", request.Dto.CategoryName);

            var existingCategory = await categoryRepository.GetByNameAsync(request.Dto.CategoryName);

            if (existingCategory != null)
            {
                // logging
                logger.LogWarning("{Entity} already exists with name {Entity} in {Operation}", "Category", request.Dto.CategoryName, nameof(CreateCategoryCommandHandler));
                throw new InvalidOperationException("Category name already exists.");
            }

            // logging
            logger.LogInformation("Creating new {Entity}", "Category");

            var createdCate = await categoryRepository.AddAsync(new Domain.Entities.Category
            {
                CategoryName = request.Dto.CategoryName,
                SLAHours = request.Dto.SLAHours,
                IsApprovalRequired = request.Dto.IsApprovalRequired,
            });

            // logging
            logger.LogInformation("Successfully created {Entity} with Id {EntityId} in {Operation}", "Category", createdCate.CategoryId, nameof(CreateCategoryCommandHandler));

            return new CategoryResponseDto
            {
                CategoryId = createdCate.CategoryId,
                CategoryName = request.Dto.CategoryName,
                IsApprovalRequired = request.Dto.IsApprovalRequired,
                IsActive = createdCate.IsActive,
                CreatedOn = createdCate.CreatedOn,
            };
        }
    }
}
