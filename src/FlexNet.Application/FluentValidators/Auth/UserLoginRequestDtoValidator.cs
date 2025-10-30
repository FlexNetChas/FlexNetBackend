using FlexNet.Application.DTOs.Auth.Request;
using FluentValidation;

namespace FlexNet.Application.FluentValidators.Auth
{
    /* FluentValidation class validate user input, handle by our controllers
     * Enteties are validated in the Infrastructure layer using Entity Framework configurations
     * It's important to validate application level (DTOs). And keep our validation in sync
     * with infrastructure layer validation constructed using EF
     * 
     * If validation is passed but the data is invalid will our exception handling catch it and been thrown
     * from application layer/our services.
     * 
     * Fluent validation handle response with proper error messages to consumer of our API
     */
    public class UserLoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
    {
        public UserLoginRequestDtoValidator()
        {
            RuleFor(user => user.Email)
                .NotEmpty()
                    .WithMessage("Email is required")
                .EmailAddress()
                    .WithMessage("Email must be a valid email address")
                .MaximumLength(100)
                    .WithMessage("Email cannot exceed 100 characters");

            RuleFor(user => user.Password)
                .NotEmpty()
                    .WithMessage("Password is required")
                .MinimumLength(6)
                    .WithMessage("Password must be at least 6 characters long")
                .MaximumLength(100)
                    .WithMessage("Password cannot exceed 100 characters")
                // We could further enhance password complexity requirements by improving our regex
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z]).+$")
                    .WithMessage("Password must contain at least one uppercase letter and one lowercase letter");
        }
    }
}
