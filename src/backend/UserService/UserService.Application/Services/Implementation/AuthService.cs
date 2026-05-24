using EscalationPlatform.Shared.Result;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Common.Interfaces;
using UserService.Application.Common.Interfaces.Identity;
using UserService.Application.DTOs;
using UserService.Application.Services.Interfaces;
using UserService.Domain;
using UserService.Domain.Entities;

namespace UserService.Application.Services.Implementation;

public sealed class AuthService : IAuthService
{
    private readonly IUserDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;
    private readonly IValidator<RegisterUserDto> _registerValidator;
    private readonly IValidator<LoginUserDto> _loginValidator;

    public AuthService(
        IUserDbContext db,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider,
        IValidator<RegisterUserDto> registerValidator,
        IValidator<LoginUserDto> loginValidator)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    public async Task<Result> RegisterAsync(RegisterUserDto dto, CancellationToken ct = default)
    {
        var validation = await _registerValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result.Failure(
                Error.ValidationFailed(validation.Errors.Select(e => e.ErrorMessage)));

        var emailExists = await _db.Users.AnyAsync(u => u.Email == dto.Email, ct);
        if (emailExists)
            return Result<string>.Failure(Error.Duplicate("Email", dto.Email));
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = _passwordHasher.Hash(dto.Password),
            Role = dto.Role,
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginUserDto dto, CancellationToken ct = default)
    {
        var validation = await _loginValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result<AuthResponse>.Failure(
                Error.ValidationFailed(validation.Errors.Select(e => e.ErrorMessage)));
        
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email, ct);

        if (user is null || !_passwordHasher.Verify(dto.Password, user.PasswordHash))
            return Result<AuthResponse>.Failure(Error.InvalidCredentials());

        if (!user.IsEmailVerified)
            return Result<AuthResponse>.Failure(Error.EmailNotVerified());

        var token = _jwtProvider.GenerateAccessToken(user);
        return Result<AuthResponse>.Success(new AuthResponse(token));
    }
}
