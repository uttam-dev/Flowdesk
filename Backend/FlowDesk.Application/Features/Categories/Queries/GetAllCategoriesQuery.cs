using FlowDesk.Application.Features.Categories.DTOs;
using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Interfaces;
using MediatR;

namespace FlowDesk.Application.Features.Categories.Queries
{
    public record GetAllCategoriesQuery(FilterCategoryDataQueryDto query) : IRequest<PagedResult<CategoryResponseDto>>;

    public class GetAllCategoriesQueryHandler(ICategoryRepository _categoryRepository) : IRequestHandler<GetAllCategoriesQuery, PagedResult<CategoryResponseDto>>
    {
        public async Task<PagedResult<CategoryResponseDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var (totalPages,categories) = await _categoryRepository.GetAllAsync(request.query);

            var mapped = categories.Select(c => new CategoryResponseDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                IsApprovalRequired = c.IsApprovalRequired,
                IsActive = c.IsActive,
                CreatedOn = c.CreatedOn,
                UpdatedOn = c.UpdatedOn
            }).ToList();

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
