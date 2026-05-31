using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Services.Interfaces;

namespace UserService.Api.Controllers;

/// <summary>
/// Внутренние эндпоинты UserService для межсервисного взаимодействия.
/// Не требуют JWT-авторизации
/// </summary>
[AllowAnonymous]
[ApiController]
[Route("internal/user")]
public sealed class InternalUserController : ControllerBase
{
    private readonly IUserService _userService;

    public InternalUserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Возвращает профиль пользователя по идентификатору.
    /// Используется SchedulerService для получения email исполнителя при SLA-нарушении.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _userService.GetByIdAsync(id, ct);

        if (!result.IsSuccess)
            return NotFound();

        return Ok(result.Value);
    }
}
