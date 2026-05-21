using EscalationPlatform.Shared.Result;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Common.Interfaces;
using UserService.Application.Common.Interfaces.Identity;
using UserService.Application.DTOs;
using UserService.Application.Services.Interfaces;
using UserService.Domain;

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

    public async Task<Result<string>> RegisterAsync(RegisterUserDto dto, CancellationToken ct = default)
    {
        var validation = await _registerValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result<string>.Failure(
                Error.ValidationFailed(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage))));

        var emailExists = await _db.Users.AnyAsync(u => u.Email == dto.Email, ct);
        if (emailExists)
            return Result<string>.Failure(Error.Duplicate("Email", dto.Email));

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = _passwordHasher.Hash(dto.Password),
            Role = dto.Role,
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return Result<string>.Success("Регистрация успешна. Подтвердите email для входа.");
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginUserDto dto, CancellationToken ct = default)
    {
        var validation = await _loginValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result<AuthResponse>.Failure(
                Error.ValidationFailed(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage))));

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email, ct);

        if (user is null || !_passwordHasher.Verify(dto.Password, user.PasswordHash))
            return Result<AuthResponse>.Failure(Error.Unauthorized("Неверный email или пароль."));

        if (!user.IsEmailVerified)
            return Result<AuthResponse>.Failure(Error.ValidationFailed("Email не подтверждён."));

        var accessToken = _jwtProvider.GenerateAccessToken(user);
        return Result<AuthResponse>.Success(new AuthResponse(accessToken));
    }
}
