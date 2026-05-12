using FlowDesk.Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Common.Validators
{
    public class CommentValidator : AbstractValidator<Comment>
    {
        public CommentValidator()
        {
            RuleFor(x => x.RequestId)
                .GreaterThan(0).WithMessage("RequestId is required");

            RuleFor(x => x.CommentById)
                .GreaterThan(0).WithMessage("CommentById is required");

            RuleFor(x => x.CommentText)
                .NotEmpty().WithMessage("Comment text is required")
                .MaximumLength(2000);

            RuleFor(x => x.CreatedOn)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("CreatedOn cannot be in future");
        }
    }
}
