using EscalationPlatform.Shared.Pagination;
using EscalationService.Application.DTOs.SlaPolicy;
using EscalationService.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EscalationService.Api.Controllers;

public sealed class SlaPolicyController : BaseController
{
    private readonly ISlaPolicyService _slaPolicyService;

    public SlaPolicyController(ISlaPolicyService slaPolicyService)
    {
        _slaPolicyService = slaPolicyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PageParams? pageParams, CancellationToken ct)
    {
        var result = await _slaPolicyService.GetAllAsync(pageParams, ct);
        return OkOrError(result);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById([FromRoute] long id, CancellationToken ct)
    {
        var result = await _slaPolicyService.GetByIdAsync(id, ct);
        return OkOrError(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSlaPolicyRequest request, CancellationToken ct)
    {
        var result = await _slaPolicyService.CreateAsync(request, ct);
        return NoContentOrError(result);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] UpdateSlaPolicyRequest request, CancellationToken ct)
    {
        var result = await _slaPolicyService.UpdateAsync(id, request, ct);
        return NoContentOrError(result);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete([FromRoute] long id, CancellationToken ct)
    {
        var result = await _slaPolicyService.DeleteAsync(id, ct);
        return NoContentOrError(result);
    }
}
