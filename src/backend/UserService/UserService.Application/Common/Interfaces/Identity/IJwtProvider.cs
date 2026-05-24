using UserService.Domain;
using UserService.Domain.Entities;

namespace UserService.Application.Common.Interfaces.Identity;

public interface IJwtProvider
{
    string GenerateAccessToken(User user);
}