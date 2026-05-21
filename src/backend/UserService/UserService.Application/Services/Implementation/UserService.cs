using EscalationPlatform.Shared.Result;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Common.Interfaces;
using UserService.Application.DTOs;
using UserService.Application.Services.Interfaces;
using UserService.Domain;

namespace UserService.Application.Services.Implementation;

public sealed class UserService : IUserService
{
    private readonly IUserDbContext _db;

    public UserService(IUserDbContext db)
    {
        _db = db;
    }

    public async Task<Result<UserResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

        if (user is null)
            return Result<UserResponse>.Failure(Error.NotFound<User>(id.ToString()));

        return Result<UserResponse>.Success(MapToResponse(user));
    }

    public async Task<Result<UserResponse>> GetCurrentAsync(Guid currentUserId, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == currentUserId, ct);

        if (user is null)
            return Result<UserResponse>.Failure(Error.NotFound<User>(currentUserId.ToString()));

        return Result<UserResponse>.Success(MapToResponse(user));
    }

    private static UserResponse MapToResponse(User user) => new(
        user.Id,
        user.FirstName,
        user.LastName,
        user.Email,
        user.PhoneNumber,
        user.ProfileImageFileName,
        user.Role,
        user.IsEmailVerified,
        user.CreatedAt);
}
