using FlowDesk.Application.Features.Categories.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Categories.Queries
{
    public record GetCategoryByIdQuery(int Id) : IRequest<CategoryResponseDto>;

    public class GetCategoryByIdQueryHandler(ICategoryRepository _categoryRepository) : IRequestHandler<GetCategoryByIdQuery, CategoryResponseDto>
    {
        public async Task<CategoryResponseDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(request.Id);
            if (category == null)
            {
                throw new KeyNotFoundException("Category not found.");
            }

            return new CategoryResponseDto
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                IsApprovalRequired = category.IsApprovalRequired,
                IsActive = category.IsActive,
                CreatedOn = category.CreatedOn,
                UpdatedOn = category.UpdatedOn
            };
        }
    }
}
