using FlexNet.Application.DTOs.Avatar.Request;
using FluentValidation;

namespace FlexNet.Application.FluentValidators.Avatar
{
    public class AvatatRequestDtoValidator : AbstractValidator<AvatarRequestDto>
    {
        public AvatatRequestDtoValidator()
        {
            RuleFor(a => a.Style)
                .NotEmpty()
                    .WithMessage("Avatar style is required")
                .MaximumLength(100)
                    .WithMessage("Avatar style cannot exceed 100 characters");

            RuleFor(a => a.Personality)
                .NotEmpty()
                    .WithMessage("Avatar personality is required")
                .MaximumLength(100)
                    .WithMessage("Avatar personality cannot exceed 100 characters");

            RuleFor(a => a.VoiceSelection)
                .NotEmpty()
                    .WithMessage("Voice selection is required")
                .MaximumLength(50)
                    .WithMessage("Voice selection cannot exceed 50 characters");
        }
    }
}
