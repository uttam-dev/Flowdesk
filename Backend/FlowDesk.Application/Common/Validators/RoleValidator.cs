using FlowDesk.Domain.Entities;
using FluentValidation;

namespace FlowDesk.Application.Common.Validators
{
    public class RoleValidator : AbstractValidator<Role>
    {
        public RoleValidator()
        {
            RuleFor(x => x.RoleName)
                .NotEmpty().WithMessage("Role name is required")
                .MaximumLength(100);

            RuleFor(x => x.RoleName)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Role name cannot be empty or whitespace");
        }
    }
}
