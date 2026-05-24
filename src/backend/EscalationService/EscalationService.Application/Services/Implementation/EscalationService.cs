using EscalationPlatform.Shared.Pagination;
using EscalationPlatform.Shared.Result;
using EscalationService.Application.Common;
using EscalationService.Application.Common.Interfaces;
using EscalationService.Application.DTOs.Escalation;
using EscalationService.Application.Extensions;
using EscalationService.Application.Services.Interfaces;
using EscalationService.Domain.Entities;
using EscalationService.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EscalationService.Application.Services.Implementation;

public sealed class EscalationService : IEscalationService
{
    private readonly IEscalationDbContext _db;
    private readonly IValidator<CreateEscalationRequest> _createValidator;
    private readonly IValidator<UpdateEscalationRequest> _updateValidator;
    private readonly ILogger<EscalationService> _logger;

    public EscalationService(
        IEscalationDbContext db,
        IValidator<CreateEscalationRequest> createValidator,
        IValidator<UpdateEscalationRequest> updateValidator,
        ILogger<EscalationService> logger)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<Result<PagedResult<EscalationSummaryResponse>>> GetAllAsync(
        EscalationFilter? filter, SortParams? sortParams, PageParams? pageParams, CancellationToken ct)
    {
        var query = _db.Escalations
            .AsNoTracking()
            .Include(e => e.SlaPolicy)
            .Include(e => e.Project)
            .Filter(filter)
            .Sort(sortParams);

        var page = await query.ToPageAsync(pageParams, ct);

        _logger.LogInformation("Retrieved {Count} escalations (page {Page})", page.TotalCount, page.Page);

        var paged = new PagedResult<EscalationSummaryResponse>(
            page.Items.Select(ToSummaryResponse).ToList(),
            page.TotalCount, page.Page, page.PageSize);

        return Result<PagedResult<EscalationSummaryResponse>>.Success(paged);
    }

    public async Task<Result<EscalationResponse>> GetByIdAsync(long id, CancellationToken ct)
    {
        var escalation = await _db.Escalations
            .AsNoTracking()
            .Include(e => e.SlaPolicy)
            .Include(e => e.Project)
            .Include(e => e.Participants)
            .Include(e => e.Criteria)
            .Include(e => e.Metrics)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (escalation is null)
        {
            _logger.LogWarning("Escalation {Id} not found", id);
            return Result<EscalationResponse>.Failure(Error.NotFound<Escalation>(id));
        }

        return Result<EscalationResponse>.Success(ToResponse(escalation));
    }

    public async Task<Result> CreateAsync(CreateEscalationRequest request, CancellationToken ct)
    {
        var validation = await _createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return Result.Failure(
                Error.ValidationFailed(validation.Errors.Select(e => e.ErrorMessage)));
        
        var slaPolicyExists = await _db.SlaPolicies.AnyAsync(p => p.Id == request.SlaPolicyId, ct);
        if (!slaPolicyExists)
            return Result.Failure(Error.NotFound<SlaPolicy>(request.SlaPolicyId));

        var projectExists = await _db.Projects.AnyAsync(p => p.Id == request.ProjectId, ct);
        if (!projectExists)
            return Result.Failure(Error.NotFound<Project>(request.ProjectId));

        var now = DateTimeOffset.UtcNow;

        var escalation = new Escalation
        {
            Name = request.Name,
            Description = request.Description,
            Status = EscalationStatus.New,
            Priority = request.Priority,
            Type = request.Type,
            AuthorId = request.AuthorId,
            SlaPolicyId = request.SlaPolicyId,
            ProjectId = request.ProjectId,
            Participants = request.Participants?.Select(p => new EscalationParticipant
            {
                UserId = p.UserId,
                Role = p.Role,
                AssignedAt = now
            }).ToList() ?? [],
            Criteria = request.Criteria?.Select(c => new Criterion
            {
                Name = c.Name,
                IsCompleted = false
            }).ToList() ?? []
        };

        _db.Escalations.Add(escalation);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created Escalation {Id} (project {ProjectId}, sla {SlaPolicyId})",
            escalation.Id, escalation.ProjectId, escalation.SlaPolicyId);

        return Result.Success();
    }

    public async Task<Result> UpdateAsync(long id, UpdateEscalationRequest request, CancellationToken ct)
    {
        var validation = await _updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return Result.Failure(
                Error.ValidationFailed(validation.Errors.Select(e => e.ErrorMessage)));
        
        var escalation = await _db.Escalations
            .Include(e => e.Participants)
            .Include(e => e.Comments)
            .Include(e => e.Metrics)
            .Include(e => e.SlaPolicy)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (escalation is null)
        {
            _logger.LogWarning("Escalation {Id} not found", id);
            return Result.Failure(Error.NotFound<Escalation>(id));
        }

        escalation.Name = request.Name;
        escalation.Description = request.Description;
        escalation.Priority = request.Priority;
        escalation.Status = request.Status;
        escalation.FirstResponseAt = request.FirstResponseAt;
        escalation.ResolvedAt = request.ResolvedAt;

        if (request.Participants is not null)
        {
            escalation.Participants.Clear();
            var now = DateTimeOffset.UtcNow;
            foreach (var p in request.Participants)
                escalation.Participants.Add(new EscalationParticipant
                {
                    UserId = p.UserId,
                    Role = p.Role,
                    AssignedAt = now
                });
        }

        if (request.Status == EscalationStatus.Closed && escalation.ClosedAt is null)
        {
            var closedAt = DateTimeOffset.UtcNow;
            escalation.ClosedAt = closedAt;

            escalation.Metrics ??= BuildMetrics(escalation, closedAt);
        }

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated Escalation {Id} → status {Status}", id, request.Status);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken ct)
    {
        var escalation = await _db.Escalations.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (escalation is null)
        {
            _logger.LogWarning("Escalation {Id} not found", id);
            return Result.Failure(Error.NotFound<Escalation>(id));
        }

        _db.Escalations.Remove(escalation);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted Escalation {Id}", id);

        return Result.Success();
    }

    /// <summary>
    /// Строим метрики при разрешении эскалации
    /// </summary>
    private static EscalationMetrics BuildMetrics(Escalation escalation, DateTimeOffset closedAt)
    {
        var firstResponseMinutes = escalation.FirstResponseAt.HasValue
            ? (int)(escalation.FirstResponseAt.Value - escalation.CreatedAt).TotalMinutes
            : 0;

        var resolutionMinutes = escalation.ResolvedAt.HasValue
            ? (int)(escalation.ResolvedAt.Value - escalation.CreatedAt).TotalMinutes
            : 0;

        var timeToCloseMinutes = (int)(closedAt - escalation.CreatedAt).TotalMinutes;

        var slaViolated = resolutionMinutes > escalation.SlaPolicy.ResolutionTimeMinutes;

        return new EscalationMetrics
        {
            EscalationId = escalation.Id,
            FirstResponseTimeMinutes = firstResponseMinutes,
            ResolutionTimeMinutes = resolutionMinutes,
            TimeToCloseMinutes = timeToCloseMinutes,
            SlaViolated = slaViolated,
            TotalComments = escalation.Comments.Count,
            TotalParticipants = escalation.Participants.Count
        };
    }

    private static EscalationSummaryResponse ToSummaryResponse(Escalation e) => new(
        e.Id,
        e.Name,
        e.Status,
        e.Priority,
        e.Type,
        e.AuthorId,
        e.SlaPolicyId,
        e.SlaPolicy.Name,
        e.ProjectId,
        e.Project.Name,
        e.ClosedAt,
        e.CreatedAt,
        e.UpdatedAt);

    private static EscalationResponse ToResponse(Escalation e) => new(
        e.Id,
        e.Name,
        e.Description,
        e.Status,
        e.Priority,
        e.Type,
        e.AuthorId,
        e.SlaPolicyId,
        e.SlaPolicy.Name,
        e.ProjectId,
        e.Project.Name,
        e.FirstResponseAt,
        e.ResolvedAt,
        e.ClosedAt,
        e.Participants.Select(p => new EscalationParticipantResponse(p.UserId, p.Role, p.AssignedAt)).ToList(),
        e.Criteria.Select(c => new CriterionResponse(c.Id, c.Name, c.IsCompleted)).ToList(),
        e.Metrics is null ? null : new EscalationMetricsResponse(
            e.Metrics.FirstResponseTimeMinutes,
            e.Metrics.ResolutionTimeMinutes,
            e.Metrics.TimeToCloseMinutes,
            e.Metrics.SlaViolated,
            e.Metrics.TotalComments,
            e.Metrics.TotalParticipants),
        e.CreatedAt,
        e.UpdatedAt);
}
