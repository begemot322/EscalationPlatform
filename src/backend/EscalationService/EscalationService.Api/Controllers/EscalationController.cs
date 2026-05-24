using EscalationPlatform.Shared.Pagination;
using EscalationService.Application.DTOs.Escalation;
using EscalationService.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EscalationService.Api.Controllers;

public sealed class EscalationController : BaseController
{
    private readonly IEscalationService _escalationService;

    public EscalationController(IEscalationService escalationService)
    {
        _escalationService = escalationService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll(
        [FromQuery] EscalationFilter? filter,
        [FromQuery] SortParams? sortParams,
        [FromQuery] PageParams? pageParams,
        CancellationToken ct)
    {
        var result = await _escalationService.GetAllAsync(filter, sortParams, pageParams, ct);
        return OkOrError(result);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById([FromRoute] long id, CancellationToken ct)
    {
        var result = await _escalationService.GetByIdAsync(id, ct);
        return OkOrError(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEscalationRequest request, CancellationToken ct)
    {
        var result = await _escalationService.CreateAsync(request, ct);
        return NoContentOrError(result);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] UpdateEscalationRequest request, CancellationToken ct)
    {
        var result = await _escalationService.UpdateAsync(id, request, ct);
        return NoContentOrError(result);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete([FromRoute] long id, CancellationToken ct)
    {
        var result = await _escalationService.DeleteAsync(id, ct);
        return NoContentOrError(result);
    }
}
