using FlowDesk.Application.Features.Categories.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Categories.Queries
{
    public record GetCategoryByIdQuery(int Id) : IRequest<CategoryResponseDto>;

    public class GetCategoryByIdQueryHandler(ICategoryRepository _categoryRepository, ILogger<GetCategoryByIdQueryHandler> logger) : IRequestHandler<GetCategoryByIdQuery, CategoryResponseDto>
    {
        public async Task<CategoryResponseDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            // logging
            logger.LogInformation("Starting {Operation} with {@Request}", nameof(GetCategoryByIdQueryHandler), request);

            // logging
            logger.LogInformation("Fetching {Entity} with Id {EntityId}", "Category", request.Id);

            var category = await _categoryRepository.GetByIdAsync(request.Id);

            if (category == null)
            {
                // logging
                logger.LogWarning("{Entity} not found with Id {EntityId} in {Operation}", "Category", request.Id, nameof(GetCategoryByIdQueryHandler));
                throw new KeyNotFoundException("Category not found.");
            }

            // logging
            logger.LogInformation("Mapping {Entity} with Id {EntityId} to DTO", "Category", request.Id);

            var result = new CategoryResponseDto
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                IsApprovalRequired = category.IsApprovalRequired,
                IsActive = category.IsActive,
                CreatedOn = category.CreatedOn,
                UpdatedOn = category.UpdatedOn
            };

            // logging
            logger.LogInformation("Successfully completed {Operation} for {Entity} with Id {EntityId}", nameof(GetCategoryByIdQueryHandler), "Category", request.Id);

            return result;
        }
    }
}
