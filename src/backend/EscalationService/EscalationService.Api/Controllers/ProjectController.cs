using EscalationPlatform.Shared.Pagination;
using EscalationService.Application.DTOs.Project;
using EscalationService.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EscalationService.Api.Controllers;

[Authorize]
[Route("api/v1/project")]
public sealed class ProjectController : BaseController
{
    private readonly IProjectService _projectService;

    public ProjectController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ProjectFilter? filter, [FromQuery] PageParams? pageParams, CancellationToken ct)
    {
        var result = await _projectService.GetAllAsync(filter, pageParams, ct);
        return OkOrError(result);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById([FromRoute] long id, CancellationToken ct)
    {
        var result = await _projectService.GetByIdAsync(id, ct);
        return OkOrError(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request, CancellationToken ct)
    {
        var result = await _projectService.CreateAsync(request, ct);
        return NoContentOrError(result);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] UpdateProjectRequest request, CancellationToken ct)
    {
        var result = await _projectService.UpdateAsync(id, request, ct);
        return NoContentOrError(result);
    }

    [HttpPatch("{id:long}/archive")]
    public async Task<IActionResult> Archive([FromRoute] long id, CancellationToken ct)
    {
        var result = await _projectService.ArchiveAsync(id, ct);
        return NoContentOrError(result);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete([FromRoute] long id, CancellationToken ct)
    {
        var result = await _projectService.DeleteAsync(id, ct);
        return NoContentOrError(result);
    }

    /// <summary>
    /// Импортирует проекты из Excel файла (.xlsx).
    /// Доступно только администраторам.
    /// Ожидаемые колонки: Название (обяз.), Код (обяз., уникален), Описание (необяз.).
    /// Проекты с кодами, уже существующими в системе, пропускаются.
    /// </summary>
    [HttpPost("import")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Import(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return Problem(statusCode: 400, title: "INVALID_FILE", detail: "Файл не передан или пустой.");

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return Problem(statusCode: 400, title: "INVALID_FORMAT", detail: "Поддерживается только формат .xlsx.");

        await using var stream = file.OpenReadStream();
        var result = await _projectService.ImportFromExcelAsync(stream, ct);
        return OkOrError(result);
    }
}
