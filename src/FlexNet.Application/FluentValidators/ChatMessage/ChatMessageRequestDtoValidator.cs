using FlexNet.Application.DTOs.ChatMessage.Request;
using FluentValidation;


namespace FlexNet.Application.FluentValidators.ChatMessage
{
    public class ChatMessageRequestDtoValidator : AbstractValidator<ChatMessageRequestDto>
    {
        public ChatMessageRequestDtoValidator()
        {
            RuleFor(c => c.MessageText)
                .NotEmpty()
                    .WithMessage("Chat message is required")
                .MaximumLength(1000)
                    .WithMessage("Chat message cannot exceed 1000 characters");
        }
    }
}
