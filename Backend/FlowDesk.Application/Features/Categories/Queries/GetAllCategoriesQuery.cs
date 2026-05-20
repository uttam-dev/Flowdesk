using FlowDesk.Application.Features.Categories.DTOs;
using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Categories.Queries
{
    public record GetAllCategoriesQuery(FilterCategoryDataQueryDto query) : IRequest<PagedResult<CategoryResponseDto>>;

    public class GetAllCategoriesQueryHandler(ICategoryRepository _categoryRepository, ILogger<GetAllCategoriesQueryHandler> logger) : IRequestHandler<GetAllCategoriesQuery, PagedResult<CategoryResponseDto>>
    {
        public async Task<PagedResult<CategoryResponseDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            // logging
            logger.LogInformation("Starting {Operation} with {@Request}", nameof(GetAllCategoriesQueryHandler), request);

            // logging
            logger.LogInformation("Fetching {Entity} list with pagination {@Request}", "Category", request.query);

            var (totalPages, categories) = await _categoryRepository.GetAllAsync(request.query);

            // logging
            logger.LogInformation("Mapping {Entity} list to DTOs", "Category");

            var mapped = categories.Select(c => new CategoryResponseDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                IsApprovalRequired = c.IsApprovalRequired,
                IsActive = c.IsActive,
                SLAHours = c.SLAHours,
                CreatedOn = c.CreatedOn,
                UpdatedOn = c.UpdatedOn
            }).ToList();

            // logging
            logger.LogInformation("Successfully completed {Operation} for {Entity} list", nameof(GetAllCategoriesQueryHandler), "Category");

            return new PagedResult<CategoryResponseDto>()
            {
                Items = mapped,
                PageNumber = request.query.PageNumber,
                PageSize = request.query.PageSize,
                TotalCount = totalPages
            };
        }
    }
}
