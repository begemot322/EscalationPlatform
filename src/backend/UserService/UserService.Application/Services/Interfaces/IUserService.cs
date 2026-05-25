using EscalationPlatform.Shared.Pagination;
using EscalationPlatform.Shared.Result;
using UserService.Application.DTOs;

namespace UserService.Application.Services.Interfaces;

public interface IUserService
{
    Task<Result<UserResponse>> GetCurrentAsync(Guid userId, CancellationToken ct);
    Task<Result<UserResponse>> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Result<PagedResult<UserResponse>>> GetAllAsync(PageParams? pageParams, CancellationToken ct);
}
