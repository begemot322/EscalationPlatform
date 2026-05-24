using UserService.Domain;
using UserService.Domain.Enums;

namespace UserService.Application.Common.Interfaces.Identity;

public interface IUserContext
{
    Guid UserId { get; }
    UserRole Role { get; }
    bool IsAuthenticated { get; }
}