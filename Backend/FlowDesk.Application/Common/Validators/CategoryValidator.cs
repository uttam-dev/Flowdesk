using FlowDesk.Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Common.Validators
{
    public class CategoryValidator : AbstractValidator<Category>
    {
        public CategoryValidator()
        {
            RuleFor(x => x.CategoryName)
                .NotEmpty().WithMessage("Category name is required")
                .MaximumLength(100);

            RuleFor(x => x.CreatedOn)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("CreatedOn cannot be in future");

            RuleFor(x => x.UpdatedOn)
                .GreaterThanOrEqualTo(x => x.CreatedOn)
                .When(x => x.UpdatedOn.HasValue)
                .WithMessage("UpdatedOn must be after CreatedOn");
        }
    }
}
