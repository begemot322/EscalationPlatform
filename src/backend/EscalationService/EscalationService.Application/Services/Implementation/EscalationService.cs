using EscalationPlatform.Shared.Messages;
using EscalationPlatform.Shared.Pagination;
using EscalationPlatform.Shared.Result;
using EscalationService.Application.Common;
using EscalationService.Application.Common.Interfaces;
using EscalationService.Application.DTOs.Escalation;
using EscalationService.Application.DTOs.Project;
using EscalationService.Application.Extensions;
using EscalationService.Application.Mappings;
using EscalationService.Application.Services.Interfaces;
using EscalationService.Domain.Entities;
using EscalationService.Domain.Enums;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EscalationService.Application.Services.Implementation;

public sealed class EscalationService : IEscalationService
{
    private readonly IEscalationDbContext _db;
    private readonly IValidator<CreateEscalationRequest> _createValidator;
    private readonly IValidator<UpdateEscalationRequest> _updateValidator;
    private readonly IUserServiceClient _userServiceClient;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<EscalationService> _logger;

    public EscalationService(
        IEscalationDbContext db,
        IValidator<CreateEscalationRequest> createValidator,
        IValidator<UpdateEscalationRequest> updateValidator,
        IUserServiceClient userServiceClient,
        IPublishEndpoint publishEndpoint,
        ILogger<EscalationService> logger)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _userServiceClient = userServiceClient;
        _publishEndpoint = publishEndpoint;
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
            page.Items.Select(EscalationMapper.ToSummaryResponse).ToList(),
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
            .Include(e => e.Comments.OrderBy(c => c.CreatedAt))
            .Include(e => e.Metrics)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (escalation is null)
        {
            _logger.LogWarning("Escalation {Id} not found", id);
            return Result<EscalationResponse>.Failure(Error.NotFound<Escalation>(id));
        }

        return Result<EscalationResponse>.Success(EscalationMapper.ToResponse(escalation));
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
            Criteria = request.Criteria?.Select((c, i) => new Criterion
            {
                Name = c.Name,
                Description = c.Description,
                IsCompleted = false,
                Order = i,
            }).ToList() ?? []
        };

        _db.Escalations.Add(escalation);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created Escalation {Id} (project {ProjectId}, sla {SlaPolicyId})",
            escalation.Id, escalation.ProjectId, escalation.SlaPolicyId);

        // Уведомляем всех назначенных участников (кроме автора — он и так знает)
        if (escalation.Participants.Count > 0)
        {
            var participantsToNotify = escalation.Participants
                .Where(p => p.UserId != request.AuthorId)
                .ToList();
            await PublishParticipantNotificationsAsync(
                escalation, participantsToNotify, ct);
        }

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
            .Include(e => e.Criteria)
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
        escalation.Type = request.Type;
        escalation.FirstResponseAt = request.FirstResponseAt;
        escalation.ResolvedAt = request.ResolvedAt;

        List<EscalationParticipant>? newParticipants = null;

        if (request.Participants is not null)
        {
            var existingIds = escalation.Participants.Select(p => p.UserId).ToHashSet();
            escalation.Participants.Clear();
            var now = DateTimeOffset.UtcNow;
            newParticipants = [];
            foreach (var p in request.Participants)
            {
                var participant = new EscalationParticipant
                {
                    UserId = p.UserId,
                    Role = p.Role,
                    AssignedAt = now
                };
                escalation.Participants.Add(participant);
                if (!existingIds.Contains(p.UserId))
                    newParticipants.Add(participant);
            }
        }

        if (request.Criteria is not null)
        {
            escalation.Criteria.Clear();
            var now = DateTimeOffset.UtcNow;
            for (var i = 0; i < request.Criteria.Count; i++)
            {
                var c = request.Criteria[i];
                escalation.Criteria.Add(new Criterion
                {
                    Name = c.Name,
                    Description = c.Description,
                    IsCompleted = c.IsCompleted,
                    CompletedAt = c.IsCompleted ? now : null,
                    Order = i,
                });
            }
        }

        if (request.Status == EscalationStatus.Closed && escalation.ClosedAt is null)
        {
            var closedAt = DateTimeOffset.UtcNow;
            escalation.ClosedAt = closedAt;

            escalation.Metrics ??= BuildMetrics(escalation, closedAt);
        }

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated Escalation {Id} → status {Status}", id, request.Status);

        // Уведомляем только новых участников
        if (newParticipants is { Count: > 0 })
            await PublishParticipantNotificationsAsync(escalation, newParticipants, ct);

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

    public async Task<Result> AddCommentAsync(long escalationId, CreateCommentRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return Result.Failure(Error.ValidationFailed(["Comment text cannot be empty."]));

        var escalation = await _db.Escalations
            .Include(e => e.Comments)
            .FirstOrDefaultAsync(e => e.Id == escalationId, ct);

        if (escalation is null)
            return Result.Failure(Error.NotFound<Escalation>(escalationId));

        var comment = new Comment
        {
            Text = request.Text.Trim(),
            IsSystem = false,
            Type = CommentType.User,
            UserId = request.AuthorId,
            EscalationId = escalationId,
        };

        _db.Comments.Add(comment);

        // Первый комментарий от не-автора фиксирует время первого ответа
        if (escalation.FirstResponseAt is null
            && request.AuthorId != escalation.AuthorId
            && escalation.Comments.All(c => c.IsSystem))
        {
            escalation.FirstResponseAt = DateTimeOffset.UtcNow;
            _logger.LogInformation(
                "FirstResponseAt set for Escalation {EscalationId} by {UserId}",
                escalationId, request.AuthorId);
        }

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Comment added to Escalation {EscalationId} by {UserId}", escalationId, request.AuthorId);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<SlaCheckItem>>> GetSlaCheckItemsAsync(CancellationToken ct)
    {
        // Только активные эскалации
        var activeStatuses = new[] { EscalationStatus.New, EscalationStatus.InProgress, EscalationStatus.WaitingForResponse };

        var escalations = await _db.Escalations
            .AsNoTracking()
            .Include(e => e.SlaPolicy)
            .Include(e => e.Project)
            .Include(e => e.Participants)
            .Where(e => activeStatuses.Contains(e.Status))
            .ToListAsync(ct);

        var items = escalations.Select(e => new SlaCheckItem(
            e.Id, e.Name, e.Project.Name, e.Priority.ToString(),
            e.SlaPolicy.ResponseTimeMinutes, e.SlaPolicy.ResolutionTimeMinutes, e.CreatedAt,
            e.Participants
                .Where(p => p.Role == ParticipantRole.Assignee)
                .Select(p => new SlaCheckParticipant(p.UserId))
                .ToList()
        )).ToList();

        _logger.LogInformation("SLA check: {Count} active escalations returned", items.Count);

        return Result<IReadOnlyList<SlaCheckItem>>.Success(items);
    }

    public async Task<Result> EscalatePriorityAsync(long id, CancellationToken ct)
    {
        var escalation = await _db.Escalations.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (escalation is null)
            return Result.Failure(Error.NotFound<Escalation>(id));

        // Повышаем приоритет на один левел, но blocker и так максимальный поэтому его смысла нет повышать
        if (escalation.Priority < EscalationPriority.Blocker)
        {
            var oldPriority = escalation.Priority;
            escalation.Priority += 1;
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Escalation {Id} priority escalated: {Old} => {New} (SLA breach)",
                id, oldPriority, escalation.Priority);
        }
        else
            _logger.LogInformation("Escalation {Id} already at max priority (Blocker), skipping escalation", id);
        
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<ProjectMonthlyStats>>> GetMonthlyStatsAsync(
        int year, int month, CancellationToken ct)
    {
        var from = new DateTimeOffset(year, month, 1, 0, 0, 0, TimeSpan.Zero);
        var to = from.AddMonths(1);
        var now = DateTimeOffset.UtcNow;

        var escalations = await _db.Escalations
            .AsNoTracking()
            .Include(e => e.SlaPolicy)
            .Include(e => e.Project)
                .ThenInclude(p => p.Owners)
            .Where(e => e.CreatedAt >= from && e.CreatedAt < to)
            .ToListAsync(ct);

        var stats = escalations
            .GroupBy(e => e.ProjectId)
            .Select(g =>
            {
                var project = g.First().Project;
                var items = g.ToList();

                var byStatus = items
                    .GroupBy(e => e.Status.ToString())
                    .ToDictionary(x => x.Key, x => x.Count());

                var byPriority = items
                    .GroupBy(e => e.Priority.ToString())
                    .ToDictionary(x => x.Key, x => x.Count());

                // Нарушение SLA: прошедшее время превышает лимит резолюции
                var slaBreaches = items.Count(e =>
                {
                    var elapsed = ((e.ResolvedAt ?? e.ClosedAt ?? now) - e.CreatedAt).TotalMinutes;
                    return elapsed > e.SlaPolicy.ResolutionTimeMinutes;
                });

                // Среднее время - только по тем, что уже решены
                var resolved = items.Where(e => e.ResolvedAt.HasValue).ToList();
                var avgHours = resolved.Count > 0
                    ? Math.Round(resolved.Average(e => (e.ResolvedAt!.Value - e.CreatedAt).TotalHours), 1)
                    : 0.0;

                var ownerIds = project.Owners
                    .Where(o => o.Role == ProjectRole.Owner)
                    .Select(o => o.UserId)
                    .ToList();

                return new ProjectMonthlyStats(
                    project.Id, project.Name, year, month,
                    items.Count, byStatus, byPriority,
                    slaBreaches, avgHours, ownerIds);
            })
            .ToList();

        _logger.LogInformation(
            "Monthly stats {Year}-{Month:D2}: {Count} projects",
            year, month, stats.Count);

        return Result<IReadOnlyList<ProjectMonthlyStats>>.Success(stats);
    }

    public async Task<Result<IReadOnlyList<UserStatsItem>>> GetUsersStatsAsync(CancellationToken ct)
    {
        var activeStatuses = new[]
            { EscalationStatus.New, EscalationStatus.InProgress, EscalationStatus.WaitingForResponse };

        // Один запрос — все эскалации с участниками и метриками
        var escalations = await _db.Escalations
            .AsNoTracking()
            .Include(e => e.Participants)
            .Include(e => e.Metrics)
            .ToListAsync(ct);

        // Группируем по исполнителям
        var byUser = escalations
            .SelectMany(e => e.Participants
                .Where(p => p.Role == ParticipantRole.Assignee)
                .Select(p => new { p.UserId, Esc = e }))
            .GroupBy(x => x.UserId)
            .ToList();

        var result = byUser.Select(group =>
        {
            var userId  = group.Key;
            var items   = group.Select(x => x.Esc).ToList();
            var total   = items.Count;

            var active    = items.Count(e => activeStatuses.Contains(e.Status));
            var closed    = items.Count(e => e.Status is EscalationStatus.Resolved or EscalationStatus.Closed);
            var cancelled = items.Count(e => e.Status == EscalationStatus.Cancelled);

            var metrics           = items.Where(e => e.Metrics is not null).Select(e => e.Metrics!).ToList();
            var closedWithMetrics = metrics.Count;

            double? avgFirstResponse = closedWithMetrics > 0
                ? Math.Round(metrics.Average(m => (double)m.FirstResponseTimeMinutes), 1) : null;
            double? avgResolution = closedWithMetrics > 0
                ? Math.Round(metrics.Average(m => (double)m.ResolutionTimeMinutes), 1) : null;

            var slaViolated  = metrics.Count(m => m.SlaViolated);
            var slaCompliant = metrics.Count(m => !m.SlaViolated);

            var effective        = Math.Max(total - cancelled, 1);
            var completionRate   = (double)closed / effective;
            var completionPercent = (int)Math.Round(completionRate * 100);

            // Efficiency: 50% закрытие + 50% SLA (если метрик нет — только закрытие)
            var slaRate = closedWithMetrics > 0 ? (double)slaCompliant / closedWithMetrics : completionRate;
            var efficiencyScore = (int)Math.Round((completionRate * 0.5 + slaRate * 0.5) * 100);

            return new UserStatsItem(
                userId, total, active, closed, cancelled,
                closedWithMetrics, avgFirstResponse, avgResolution,
                slaViolated, slaCompliant, completionPercent, efficiencyScore);
        }).ToList();

        _logger.LogInformation("UsersStats: {Count} assignees computed", result.Count);
        return Result<IReadOnlyList<UserStatsItem>>.Success(result);
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

    /// <summary>
    /// Резолвит email участников через UserService и публикует ParticipantAssigned для каждого.
    /// </summary>
    private async Task PublishParticipantNotificationsAsync(
        Escalation escalation,
        IReadOnlyList<EscalationParticipant> participants,
        CancellationToken ct)
    {
        try
        {
            var userIds = participants.Select(p => p.UserId).Distinct().ToList();
            var users = await _userServiceClient.GetByIdsAsync(userIds, ct);
            var userMap = users.ToDictionary(u => u.Id);

            var projectName = escalation.Project?.Name ?? string.Empty;
            var now = DateTimeOffset.UtcNow;

            foreach (var participant in participants)
            {
                if (!userMap.TryGetValue(participant.UserId, out var user)) continue;

                var roleLabel = participant.Role switch
                {
                    ParticipantRole.Assignee => "Исполнитель",
                    ParticipantRole.Observer => "Наблюдатель",
                    ParticipantRole.Approver => "Согласующий",
                    _ => participant.Role.ToString()
                };

                await _publishEndpoint.Publish(new ParticipantAssigned(
                    EscalationId:      escalation.Id,
                    EscalationName:    escalation.Name,
                    ProjectName:       projectName,
                    RecipientEmail:    user.Email,
                    RecipientFirstName: user.FirstName,
                    Role:              roleLabel,
                    AssignedAt:        now), ct);

                _logger.LogInformation(
                    "ParticipantAssigned published: escalation {Id} → {Email} ({Role})",
                    escalation.Id, user.Email, roleLabel);
            }
        }
        catch (Exception ex)
        {
            // Не прерываем основную операцию из-за ошибки уведомлений
            _logger.LogError(ex,
                "Failed to publish ParticipantAssigned for escalation {Id}", escalation.Id);
        }
    }

}
