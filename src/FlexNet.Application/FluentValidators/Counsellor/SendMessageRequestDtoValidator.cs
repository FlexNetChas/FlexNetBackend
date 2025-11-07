using FlexNet.Application.DTOs.Counsellor.Request;
using FluentValidation;

namespace FlexNet.Application.FluentValidators.Counsellor
{
    public class SendMessageRequestDtoValidator : AbstractValidator<SendMessageRequestDto>
    {
        public SendMessageRequestDtoValidator()
        {
            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Message is required.")
                .MaximumLength(1000).WithMessage("Message cannot exceed 1000 characters.");

            RuleFor(x => x.ChatSessionId)
                .GreaterThan(0)
                .When(x => x.ChatSessionId.HasValue)
                .WithMessage("ChatSessionId must be a positive number when provided.");
        }
    }
}
