using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using UserService.Application.Common.Interfaces.Identity;
using UserService.Domain;
using UserService.Domain.Enums;

namespace UserService.Infrastructure.Identity;

public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId =>
        Guid.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    public UserRole Role =>
        Enum.Parse<UserRole>(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.Role)!);

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
