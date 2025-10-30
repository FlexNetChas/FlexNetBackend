using FlexNet.Application.DTOs.ChatSession.Request;
using FluentValidation;

namespace FlexNet.Application.FluentValidators.ChatSession
{
    public class CreateChatSessionRequestDtoValidator : AbstractValidator<CreateChatSessionRequestDto>
    {
        public CreateChatSessionRequestDtoValidator()
        {
            RuleFor(x => x.Summary)
                .MaximumLength(1000)
                .WithMessage("Summary cannot exceed 1000 characters");

            RuleFor(x => x.StartedTime)
                .NotEmpty()
                    .WithMessage("Started time is required")
                .LessThanOrEqualTo(DateTime.UtcNow)
                    .WithMessage("Started time cannot be in the future");

            RuleFor(x => x.EndedTime)
                .GreaterThan(x => x.StartedTime)
                    .When(x => x.EndedTime.HasValue)
                    .WithMessage("Ended time must be after started time");
        }
    }
}
