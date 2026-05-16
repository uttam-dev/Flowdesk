using FlowDesk.Application.Features.Categories.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Categories.Commands
{
    public record CreateCategoryCommand(CreateCategoryDto Dto)
    : IRequest<CategoryResponseDto>;

    public class CreateCategoryCommandHandler(ICategoryRepository categoryRepository) : IRequestHandler<CreateCategoryCommand, CategoryResponseDto>
    {
        public async Task<CategoryResponseDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {

            var existingCategory = await categoryRepository.GetByNameAsync(request.Dto.CategoryName);
            
            if(existingCategory != null)
            {
                throw new InvalidOperationException("Category name already exists.");
            }

            var createdCate = await categoryRepository.AddAsync(new Domain.Entities.Category
            {
                CategoryName = request.Dto.CategoryName,
                IsApprovalRequired = request.Dto.IsApprovalRequired,
            });

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
