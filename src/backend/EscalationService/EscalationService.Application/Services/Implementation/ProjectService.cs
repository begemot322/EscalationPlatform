using EscalationPlatform.Shared.Pagination;
using EscalationPlatform.Shared.Result;
using EscalationService.Application.Common;
using EscalationService.Application.Common.Interfaces;
using EscalationService.Application.DTOs.Project;
using EscalationService.Application.Services.Interfaces;
using EscalationService.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EscalationService.Application.Services.Implementation;

public sealed class ProjectService : IProjectService
{
    private readonly IEscalationDbContext _db;
    private readonly IValidator<CreateProjectRequest> _createValidator;
    private readonly IValidator<UpdateProjectRequest> _updateValidator;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(
        IEscalationDbContext db,
        IValidator<CreateProjectRequest> createValidator,
        IValidator<UpdateProjectRequest> updateValidator,
        ILogger<ProjectService> logger)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

public async Task<Result<PagedResult<ProjectResponse>>> GetAllAsync(ProjectFilter? filter, PageParams? pageParams, CancellationToken ct = default)
    {
        var query = _db.Projects.AsNoTracking();

        if (filter?.IsActive.HasValue == true)
            query = query.Where(p => p.IsActive == filter.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(filter?.Search))
            query = query.Where(p => p.Name.Contains(filter.Search) || p.Code.Contains(filter.Search));

        var result = await query.OrderBy(p => p.Name).ToPageAsync(pageParams, ct);

        _logger.LogInformation("Retrieved {Count} projects (page {Page})", result.TotalCount, result.Page);

        var paged = new PagedResult<ProjectResponse>(
            result.Items.Select(ToResponse).ToList(),
            result.TotalCount, result.Page, result.PageSize);

        return Result<PagedResult<ProjectResponse>>.Success(paged);
    }

    public async Task<Result<ProjectResponse>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var project = await _db.Projects
            .AsNoTracking()
            .Include(p => p.Owners)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (project is null)
        {
            _logger.LogWarning("Project {Id} not found", id);
            return Result<ProjectResponse>.Failure(Error.NotFound<Project>(id));
        }

        return Result<ProjectResponse>.Success(ToResponse(project));
    }

    public async Task<Result> CreateAsync(CreateProjectRequest request, CancellationToken ct = default)
    {
        var validation = await _createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return Result.Failure(
                Error.ValidationFailed(validation.Errors.Select(e => e.ErrorMessage)));
        
        var now = DateTimeOffset.UtcNow; 

        var project = new Project
        {
            Name = request.Name,
            Description = request.Description,
            Code = await GenerateUniqueCodeAsync(ct),
            IsActive = true,
            Owners = request.Owners?.Select(o => new ProjectOwner
            {
                UserId = o.UserId,
                Role = o.Role,
                AssignedAt = now
            }).ToList() ?? []
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created Project {Id} with code '{Code}', owners: {OwnerCount}", project.Id, project.Code, project.Owners.Count);

        return Result.Success();
    }

    public async Task<Result> UpdateAsync(long id, UpdateProjectRequest request, CancellationToken ct = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return Result.Failure(
                Error.ValidationFailed(validation.Errors.Select(e => e.ErrorMessage)));
        
        var project = await _db.Projects
            .Include(p => p.Owners)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (project is null)
        {
            _logger.LogWarning("Project {Id} not found", id);
            return Result.Failure(Error.NotFound<Project>(id));
        }

        project.Name = request.Name;
        project.Description = request.Description;

        if (request.Owners is not null)
        {
            project.Owners.Clear();
            var now = DateTimeOffset.UtcNow;
            foreach (var o in request.Owners)
                project.Owners.Add(new ProjectOwner { UserId = o.UserId, Role = o.Role, AssignedAt = now });
        }

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated Project {Id}", id);

        return Result.Success();
    }

    public async Task<Result> ArchiveAsync(long id, CancellationToken ct = default)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (project is null)
        {
            _logger.LogWarning("Project {Id} not found", id);
            return Result.Failure(Error.NotFound<Project>(id));
        }

        project.IsActive = false;
        project.ArchivedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Archived Project {Id}", id);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken ct = default)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (project is null)
        {
            _logger.LogWarning("Project {Id} not found", id);
            return Result.Failure(Error.NotFound<Project>(id));
        }

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted Project {Id}", id);

        return Result.Success();
    }

    private async Task<string> GenerateUniqueCodeAsync(CancellationToken ct)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string code;
        do
        {
            code = new string(Enumerable.Range(0, 10)
                .Select(_ => chars[Random.Shared.Next(chars.Length)])
                .ToArray());
        }
        while (await _db.Projects.AnyAsync(p => p.Code == code, ct));

        return code;
    }

    private static ProjectResponse ToResponse(Project p) => new(
        p.Id, p.Name, p.Description, p.Code,
        p.IsActive, p.ArchivedAt, p.CreatedAt, p.UpdatedAt,
        p.Owners.Select(o => new ProjectOwnerResponse(o.UserId, o.Role, o.AssignedAt)).ToList());
}