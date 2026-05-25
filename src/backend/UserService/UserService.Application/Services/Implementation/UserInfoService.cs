using EscalationPlatform.Shared.Pagination;
using EscalationPlatform.Shared.Result;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Common.Interfaces;
using UserService.Application.DTOs;
using UserService.Application.Services.Interfaces;
using UserService.Domain.Entities;

namespace UserService.Application.Services.Implementation;

public sealed class UserInfoService : IUserService
{
    private readonly IUserDbContext _db;

    public UserInfoService(IUserDbContext db)
    {
        _db = db;
    }

    public async Task<Result<UserResponse>> GetCurrentAsync(Guid userId, CancellationToken ct)
        => await GetByIdAsync(userId, ct);

    public async Task<Result<UserResponse>> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        if (user is null)
            return Result<UserResponse>.Failure(Error.NotFound<User>(id));

        return Result<UserResponse>.Success(MapToResponse(user));
    }

    public async Task<Result<PagedResult<UserResponse>>> GetAllAsync(PageParams? pageParams, CancellationToken ct)
    {
        var query = _db.Users.AsNoTracking().OrderBy(u => u.LastName).ThenBy(u => u.FirstName);
        var page = await query.ToPageAsync(pageParams, ct);

        var result = new PagedResult<UserResponse>(
            page.Items.Select(MapToResponse).ToList(),
            page.TotalCount, page.Page, page.PageSize);

        return Result<PagedResult<UserResponse>>.Success(result);
    }

    private static UserResponse MapToResponse(User user) => new(
        user.Id,
        user.FirstName,
        user.LastName,
        user.Email,
        user.Role,
        user.WorkType,
        user.ProfileImageFileName,
        user.IsEmailVerified,
        user.CreatedAt);
}
