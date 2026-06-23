using EscalationPlatform.Shared.Pagination;
using EscalationPlatform.Shared.Result;
using EscalationService.Application.Common;
using EscalationService.Application.Common.Interfaces;
using EscalationService.Application.DTOs.Project;
using EscalationService.Application.Excel;
using EscalationService.Application.Mappings;
using EscalationService.Application.Services.Interfaces;
using EscalationService.Domain.Entities;
using EscalationService.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EscalationService.Application.Services.Implementation;

public sealed class ProjectService : IProjectService
{
    private readonly IEscalationDbContext _db;
    private readonly IValidator<CreateProjectRequest> _createValidator;
    private readonly IValidator<UpdateProjectRequest> _updateValidator;
    private readonly IExcelReaderService _excelReader;
    private readonly IUserServiceClient _userServiceClient;
    private readonly ILogger<ProjectService> _logger;

    // Колонки Excel для импорта проектов
    private static readonly List<ColumnDefinition> ImportColumns =
    [
        new("Название", isRequired: true, isUnique: false),
        new("Код", isRequired: true,  isUnique: true),
        new("Описание", isRequired: false, isUnique: false),
        new("Владелец (email)", isRequired: false, isUnique: false)
    ];

    public ProjectService(
        IEscalationDbContext db,
        IValidator<CreateProjectRequest> createValidator,
        IValidator<UpdateProjectRequest> updateValidator,
        IExcelReaderService excelReader,
        IUserServiceClient userServiceClient,
        ILogger<ProjectService> logger)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _excelReader = excelReader;
        _userServiceClient = userServiceClient;
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
            result.Items.Select(ProjectMapper.ToResponse).ToList(),
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

        return Result<ProjectResponse>.Success(ProjectMapper.ToResponse(project));
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

    public async Task<Result<ProjectImportResult>> ImportFromExcelAsync(Stream fileStream, CancellationToken ct)
    {
        // 1. Читаем и валидируем Excel
        var readResult = _excelReader.Read(fileStream, ImportColumns);

        if (!readResult.IsSuccess)
        {
            var errors =
                readResult.StructureErrors
                    .Select(e => $"Отсутствует обязательная колонка «{e.ColumnName}»")
                .Concat(readResult.EmptyValueErrors
                    .Select(e => $"Строка {e.RowNumber}: не заполнено обязательное поле «{e.ColumnName}»"))
                .Concat(readResult.DuplicateValueErrors
                    .Select(e => $"Строка {e.RowNumber}: дублирующееся значение в колонке «{e.ColumnName}»"));

            return Result<ProjectImportResult>.Failure(
                Error.ValidationFailed(errors));
        }

        if (readResult.Rows.Count == 0)
            return Result<ProjectImportResult>.Success(new ProjectImportResult(0, []));

        // 2. Коды из файла → проверяем дубли с БД
        var codesInFile = readResult.Rows
            .Select(r => r["Код"]?.ToString()?.Trim().ToUpperInvariant() ?? string.Empty)
            .Where(c => !string.IsNullOrEmpty(c))
            .ToList();

        var existingCodes = await _db.Projects
            .Where(p => codesInFile.Contains(p.Code.ToUpper()))
            .Select(p => p.Code.ToUpper())
            .ToListAsync(ct);

        var existingSet = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

        // 3. Собираем все email владельцев из файла и резолвим их через UserService
        var allEmails = readResult.Rows
            .Select(r => r["Владелец (email)"]?.ToString()?.Trim() ?? string.Empty)
            .Where(e => !string.IsNullOrEmpty(e))
            .SelectMany(e => e.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var usersByEmail = allEmails.Count > 0
            ? (await _userServiceClient.GetByEmailsAsync(allEmails, ct))
              .ToDictionary(u => u.Email, u => u.Id, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        var skipped = new List<string>();
        var toInsert = new List<Project>();
        var now = DateTimeOffset.UtcNow;

        foreach (var row in readResult.Rows)
        {
            var code = row["Код"]?.ToString()?.Trim() ?? string.Empty;
            var name = row["Название"]?.ToString()?.Trim() ?? string.Empty;
            var description = row["Описание"]?.ToString()?.Trim();
            var ownerEmailsRaw = row["Владелец (email)"]?.ToString()?.Trim() ?? string.Empty;

            if (existingSet.Contains(code))
            {
                skipped.Add(code);
                continue;
            }

            // Резолвим владельцев — только те email, что нашлись в UserService
            var owners = ownerEmailsRaw
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(e => usersByEmail.ContainsKey(e))
                .Select(e => new ProjectOwner
                {
                    UserId = usersByEmail[e],
                    Role = ProjectRole.Owner,
                    AssignedAt = now,
                })
                .ToList();

            toInsert.Add(new Project
            {
                Name = name,
                Description = string.IsNullOrWhiteSpace(description) ? null : description,
                Code = code.ToUpperInvariant(),
                IsActive = true,
                Owners = owners,
                CreatedAt = now,
                UpdatedAt = now,
            });
        }

        if (toInsert.Count > 0)
        {
            await _db.Projects.AddRangeAsync(toInsert, ct);
            await _db.SaveChangesAsync(ct);
        }

        _logger.LogInformation(
            "Excel import: {Imported} projects imported, {Skipped} skipped (duplicate codes)",
            toInsert.Count, skipped.Count);

        return Result<ProjectImportResult>.Success(
            new ProjectImportResult(toInsert.Count, skipped));
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

}