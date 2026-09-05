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
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _userService.GetByIdAsync(id, ct);
        if (!result.IsSuccess) return NotFound();
        return Ok(result.Value);
    }

    /// <summary>
    /// Возвращает пользователей по списку email-адресов.
    /// Используется EscalationService при импорте проектов из Excel.
    /// </summary>
    [HttpPost("by-emails")]
    public async Task<IActionResult> GetByEmails([FromBody] List<string> emails, CancellationToken ct)
    {
        var users = await _userService.GetByEmailsAsync(emails, ct);
        return Ok(users);
    }

    /// <summary>
    /// Возвращает пользователей по списку идентификаторов.
    /// Используется EscalationService при отправке уведомлений о назначении.
    /// </summary>
    [HttpPost("by-ids")]
    public async Task<IActionResult> GetByIds([FromBody] List<Guid> ids, CancellationToken ct)
    {
        var users = await _userService.GetByIdsAsync(ids, ct);
        return Ok(users);
    }
}
