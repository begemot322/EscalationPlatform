using EscalationPlatform.Shared.Result;
using UserService.Application.DTOs;

namespace UserService.Application.Services.Interfaces;

public interface IAuthService
{
    Task<Result> RegisterAsync(RegisterUserDto dto, CancellationToken ct = default);
    Task<Result<AuthResponse>> LoginAsync(LoginUserDto dto, CancellationToken ct = default);
}
