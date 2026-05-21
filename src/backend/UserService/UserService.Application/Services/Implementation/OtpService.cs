using EscalationPlatform.Shared.Messages;
using EscalationPlatform.Shared.Result;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Common.Interfaces;
using UserService.Application.Common.Interfaces.Identity;
using UserService.Application.DTOs;
using UserService.Application.Services.Interfaces;
using UserService.Domain;

namespace UserService.Application.Services.Implementation;

public sealed class OtpService : IOtpService
{
    private const int OtpExpiresMinutes = 3;
    private const int ResendCooldownSeconds = 60;

    private readonly IUserDbContext _db;
    private readonly IJwtProvider _jwtProvider;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IValidator<VerifyOtpDto> _verifyValidator;

    public OtpService(
        IUserDbContext db,
        IJwtProvider jwtProvider,
        IPublishEndpoint publishEndpoint,
        IValidator<VerifyOtpDto> verifyValidator)
    {
        _db = db;
        _jwtProvider = jwtProvider;
        _publishEndpoint = publishEndpoint;
        _verifyValidator = verifyValidator;
    }

    public async Task<Result<OtpSentResponse>> SendAsync(string email, CancellationToken ct = default)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        if (user is null)
            return Result<OtpSentResponse>.Failure(Error.NotFound<User>(email));

        if (user.IsEmailVerified)
            return Result<OtpSentResponse>.Failure(Error.ValidationFailed("Email уже подтверждён."));

        var existing = await _db.OtpCodes
            .Where(o => o.UserId == user.Id)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (existing is not null
            && existing.CreatedAt.AddSeconds(ResendCooldownSeconds) > DateTime.UtcNow)
        {
            var secondsLeft = (int)(existing.CreatedAt.AddSeconds(ResendCooldownSeconds)
                                    - DateTime.UtcNow).TotalSeconds;
            return Result<OtpSentResponse>.Failure(
                Error.ValidationFailed($"Повторная отправка доступна через {secondsLeft} сек."));
        }

        var code = GenerateOtpCode();
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(OtpExpiresMinutes);
        var canResendAt = now.AddSeconds(ResendCooldownSeconds);

        if (existing is not null)
        {
            existing.Code = code;
            existing.CreatedAt = now;
            existing.ExpiresAt = expiresAt;
            existing.IsUsed = false;
        }
        else
        {
            _db.OtpCodes.Add(new OtpCode
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Email = email,
                Code = code,
                CreatedAt = now,
                ExpiresAt = expiresAt,
                IsUsed = false
            });
        }

        await _db.SaveChangesAsync(ct);

        await _publishEndpoint.Publish(
            new OtpCodeRequested(email, user.FirstName, code, OtpExpiresMinutes), ct);

        return Result<OtpSentResponse>.Success(new OtpSentResponse(expiresAt, canResendAt));
    }

    public async Task<Result<AuthResponse>> VerifyAsync(VerifyOtpDto dto, CancellationToken ct = default)
    {
        var validation = await _verifyValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result<AuthResponse>.Failure(
                Error.ValidationFailed(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage))));

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email, ct);

        if (user is null)
            return Result<AuthResponse>.Failure(Error.NotFound<User>(dto.Email));

        if (user.IsEmailVerified)
            return Result<AuthResponse>.Failure(Error.ValidationFailed("Email уже подтверждён."));

        var otp = await _db.OtpCodes
            .Where(o => o.UserId == user.Id
                        && o.Code == dto.Code
                        && !o.IsUsed
                        && o.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync(ct);

        if (otp is null)
            return Result<AuthResponse>.Failure(Error.ValidationFailed("Неверный или истёкший код."));

        otp.IsUsed = true;
        user.IsEmailVerified = true;
        await _db.SaveChangesAsync(ct);

        var accessToken = _jwtProvider.GenerateAccessToken(user);
        return Result<AuthResponse>.Success(new AuthResponse(accessToken));
    }

    private static string GenerateOtpCode()
        => Random.Shared.Next(100_000, 999_999).ToString();
}
