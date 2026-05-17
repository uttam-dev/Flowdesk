using AutoMapper;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Features.Categories.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Categories.Commands
{
    public record UpdateCategoryCommand(int Id, UpdateCategoryDto Dto)
     : IRequest<CategoryResponseDto>;

    public class UpdateCategoryCommandHandler(ICategoryRepository _repo, IMapper _mapper, ILogger<UpdateCategoryCommandHandler> logger)
: IRequestHandler<UpdateCategoryCommand, CategoryResponseDto>
    {
        public async Task<CategoryResponseDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            // logging
            logger.LogInformation("Starting {Operation} with {@Request}", nameof(UpdateCategoryCommandHandler), request);

            // logging
            logger.LogInformation("Fetching {Entity} with Id {EntityId}", "Category", request.Id);

            var category = await _repo.GetByIdAsync(request.Id);

            if (category == null)
            {
                // logging
                logger.LogWarning("{Entity} not found with Id {EntityId} in {Operation}", "Category", request.Id, nameof(UpdateCategoryCommandHandler));
                throw new NotFoundException("Category not found");
            }

            // logging
            logger.LogInformation("Mapping updates to {Entity} with Id {EntityId}", "Category", request.Id);

            _mapper.Map(request.Dto, category);

            category.UpdatedOn = DateTime.UtcNow;

            // logging
            logger.LogInformation("Updating {Entity} with Id {EntityId}", "Category", request.Id);

            var updated = await _repo.Update(category);

            // logging
            logger.LogInformation("Successfully completed {Operation} for {Entity} with Id {EntityId}", nameof(UpdateCategoryCommandHandler), "Category", request.Id);

            return _mapper.Map<CategoryResponseDto>(updated);
        }
    }
}
