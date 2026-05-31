using EscalationService.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EscalationService.Api.Controllers;

/// <summary>
/// ВНУТРЕННИЕ ЭНДПОИНТЫ
/// Нет проксируется через API Gateway, доступен только внутри сети.
/// </summary>
[Route("internal/escalation")]
public sealed class InternalEscalationController : BaseController
{
    private readonly IEscalationService _escalationService;

    public InternalEscalationController(IEscalationService escalationService)
    {
        _escalationService = escalationService;
    }

    /// <summary>
    /// Возвращает активные эскалации с SLA-данными для SchedulerService.
    /// </summary>
    [HttpGet("sla-check")]
    public async Task<IActionResult> GetSlaCheckItems(CancellationToken ct)
    {
        var result = await _escalationService.GetSlaCheckItemsAsync(ct);
        return OkOrError(result);
    }

    /// <summary>
    /// Повышает приоритет эскалации на один левел (вызывается при нарушении SLA).
    /// </summary>
    [HttpPatch("{id:long}/escalate-priority")]
    public async Task<IActionResult> EscalatePriority([FromRoute] long id, CancellationToken ct)
    {
        var result = await _escalationService.EscalatePriorityAsync(id, ct);
        return NoContentOrError(result);
    }

    /// <summary>
    /// Возвращает месячную статистику по всем проектам для формирования отчетов.
    /// </summary>
    [HttpGet("monthly-stats")]
    public async Task<IActionResult> GetMonthlyStats(
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken ct)
    {
        var result = await _escalationService.GetMonthlyStatsAsync(year, month, ct);
        return OkOrError(result);
    }

}
