using EscalationPlatform.Shared.Result;
using UserService.Application.DTOs;

namespace UserService.Application.Services.Interfaces;

public interface IUserService
{
    Task<Result<UserResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<UserResponse>> GetCurrentAsync(Guid currentUserId, CancellationToken ct = default);
}
