using UserService.Domain;

namespace UserService.Application.Common.Interfaces.Identity;

public interface IJwtProvider
{
    string GenerateAccessToken(User user);
}