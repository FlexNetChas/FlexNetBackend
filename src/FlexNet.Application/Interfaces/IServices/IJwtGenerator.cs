using FlexNet.Domain.Entities;

namespace FlexNet.Application.Interfaces.IServices;

public interface IJwtGenerator
{
   string GenerateAccessToken(User user);
}