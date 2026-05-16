using AutoMapper;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Features.Categories.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;

namespace FlowDesk.Application.Features.Categories.Commands
{
    public record UpdateCategoryCommand(int Id, UpdateCategoryDto Dto)
     : IRequest<CategoryResponseDto>;

    public class UpdateCategoryCommandHandler(ICategoryRepository _repo, IMapper _mapper)
    : IRequestHandler<UpdateCategoryCommand, CategoryResponseDto>
    {
        public async Task<CategoryResponseDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _repo.GetByIdAsync(request.Id);
            if (category == null)
                throw new NotFoundException("Category not found");

            _mapper.Map(request.Dto, category); 

            category.UpdatedOn = DateTime.UtcNow;

            var updated = await _repo.Update(category);

            return _mapper.Map<CategoryResponseDto>(updated);
        }
    }
}
