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

            // Todo:
            // Validate age base on our program user restrictions. For now, accept null or age between 0 and 100
            RuleFor(x => x.Age)
                .Must(age => age == null || (age >= 0 && age <= 100));
        }
    }
}
