using FlexNet.Application.DTOs.Auth.Request;
using FluentValidation;

namespace FlexNet.Application.FluentValidators.Auth
{
    /* Validates refresh token request DTOs to ensure the refresh token is provided
     * Business rules (like checking if token exists, is expired, or already used) are handled in TokenService */
    public class RefreshTokenValidator : AbstractValidator<RefreshRequestDto>
    {
        public RefreshTokenValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty()
                    .WithMessage("Refresh token is required");
        }
    }
}
