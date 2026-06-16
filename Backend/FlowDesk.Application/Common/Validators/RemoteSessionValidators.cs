using FlowDesk.Application.Features.Remote.Commands;
using FlowDesk.Application.Features.Remote.DTOs;
using FluentValidation;

namespace FlowDesk.Application.Features.Remote.Validators;

public class InitiateRemoteSessionDtoValidator : AbstractValidator<InitiateRemoteSessionDto>
{
    public InitiateRemoteSessionDtoValidator()
    {
        RuleFor(x => x.RequestId)
            .GreaterThan(0).WithMessage("RequestId must be a valid positive integer.");
    }
}

public class RespondToRemoteSessionDtoValidator : AbstractValidator<RespondToRemoteSessionDto>
{
    public RespondToRemoteSessionDtoValidator()
    {
        // RejectionReason is required only when rejecting
        When(x => !x.Accepted, () =>
        {
            RuleFor(x => x.RejectionReason)
                .NotEmpty().WithMessage("Please provide a reason for rejecting the remote access request.")
                .MaximumLength(500).WithMessage("Rejection reason cannot exceed 500 characters.");
        });
    }
}

public class EndRemoteSessionDtoValidator : AbstractValidator<EndRemoteSessionDto>
{
    public EndRemoteSessionDtoValidator()
    {
        RuleFor(x => x.ResolutionNotes)
            .MaximumLength(2000).WithMessage("Resolution notes cannot exceed 2000 characters.")
            .When(x => x.ResolutionNotes is not null);
    }
}
