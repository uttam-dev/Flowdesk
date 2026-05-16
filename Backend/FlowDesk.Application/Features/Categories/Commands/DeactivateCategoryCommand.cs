using AutoMapper;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Features.Categories.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Categories.Commands
{
    public record DeactivateCategoryCommand(int Id)
        : IRequest<CategoryResponseDto>;

    public class DeactivateCategoryCommandHandler(ICategoryRepository _repo, IMapper _mapper)
        : IRequestHandler<DeactivateCategoryCommand, CategoryResponseDto>
    {
        public async Task<CategoryResponseDto> Handle(DeactivateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _repo.GetByIdAsync(request.Id);
            if (category == null)
                throw new NotFoundException("Category not found");

            category.IsActive = false;
            category.UpdatedOn = DateTime.UtcNow;

            var updated = await _repo.Update(category);

            return _mapper.Map<CategoryResponseDto>(updated);
        }
    }
}
