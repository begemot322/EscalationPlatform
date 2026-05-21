using UserService.Domain;

namespace UserService.Application.Common.Interfaces.Identity;

public interface IUserContext
{
    Guid UserId { get; }
    UserRole Role { get; }
    bool IsAuthenticated { get; }
}