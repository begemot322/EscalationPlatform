using EscalationPlatform.Shared.Result;
using UserService.Application.DTOs;

namespace UserService.Application.Services.Interfaces;

public interface IOtpService
{
    Task<Result<OtpSentResponse>> SendAsync(string email, CancellationToken ct = default);
    Task<Result<AuthResponse>> VerifyAsync(VerifyOtpDto dto, CancellationToken ct = default);
}