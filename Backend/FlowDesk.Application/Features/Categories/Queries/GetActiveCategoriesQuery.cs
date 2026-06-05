using FlowDesk.Application.Features.Categories.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;


namespace FlowDesk.Application.Features.Categories.Queries
{
     public record GetActiveCategoriesQuery : IRequest<List<CategoryBasicResponseDto>>;

    public class GetActiveCategoriesQueryHandler(ICategoryRepository _categoryRepository) : IRequestHandler<GetActiveCategoriesQuery, List<CategoryBasicResponseDto>>
    {
        public async Task<List<CategoryBasicResponseDto>> Handle(GetActiveCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _categoryRepository.GetAllActiveAsync();
            
            return categories.Select(c => new CategoryBasicResponseDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName
            }).ToList();
        }
    }
}
