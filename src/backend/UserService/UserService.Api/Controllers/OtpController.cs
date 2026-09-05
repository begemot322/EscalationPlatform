using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs;
using UserService.Application.Services.Interfaces;

namespace UserService.Api.Controllers;

[AllowAnonymous]
public class OtpController : BaseController
{
    private readonly IOtpService _otpService;

    public OtpController(IOtpService otpService)
    {
        _otpService = otpService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromQuery] string email, CancellationToken ct)
    {
        var result = await _otpService.SendAsync(email, ct);
        return OkOrError(result);
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyOtpDto dto, CancellationToken ct)
    {
        var result = await _otpService.VerifyAsync(dto, ct);
        return OkOrError(result);
    }
}
