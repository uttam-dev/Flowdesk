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
    public record ActivateCategoryCommand(int Id)
      : IRequest<CategoryResponseDto>;

    public class ActivateCategoryCommandHandler(ICategoryRepository _repo, IMapper _mapper)
    : IRequestHandler<ActivateCategoryCommand, CategoryResponseDto>
    {
        public async Task<CategoryResponseDto> Handle(ActivateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _repo.GetByIdAsync(request.Id);
            if (category == null)
                throw new NotFoundException("Category not found");

            category.IsActive = true;
            category.UpdatedOn = DateTime.UtcNow;

            var updated = await _repo.Update(category);

            return _mapper.Map<CategoryResponseDto>(updated);
        }
    }
}
