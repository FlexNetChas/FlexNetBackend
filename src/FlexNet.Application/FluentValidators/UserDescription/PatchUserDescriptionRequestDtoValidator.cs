using FlexNet.Application.DTOs.UserDescription.Request;
using FluentValidation;

namespace FlexNet.Application.FluentValidators.UserDescription
{
    public class PatchUserDescriptionRequestDtoValidator : AbstractValidator<PatchUserDescriptionRequestDto>
    {
        public PatchUserDescriptionRequestDtoValidator()
        {
            RuleFor(x => x.Age)
                .Must(age => age == null || (age >= 0 && age <= 100))
                .WithMessage("Age must be between 0 and 100 if provided.");

            RuleFor(x => x.Gender)
                .MaximumLength(20)
                .WithMessage("Gender cannot exceed 20 characters.");

            RuleFor(x => x.Education)
                .MaximumLength(50)
                .WithMessage("Education cannot exceed 50 characters.");

            RuleFor(x => x.Purpose)
                .MaximumLength(50)
                .WithMessage("Purpose cannot exceed 50 characters.");
        }
    }
}
