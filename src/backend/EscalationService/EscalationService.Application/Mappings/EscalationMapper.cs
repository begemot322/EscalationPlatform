using EscalationService.Application.DTOs.Escalation;
using EscalationService.Domain.Entities;

namespace EscalationService.Application.Mappings;

public static class EscalationMapper
{
    public static EscalationSummaryResponse ToSummaryResponse(Escalation e) => new(
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

    public static EscalationResponse ToResponse(Escalation e) => new(
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
        e.Criteria.Select(c => new CriterionResponse(c.Id, c.Name, c.Description, c.IsCompleted)).ToList(),
        e.Comments.Select(c => new CommentResponse(c.Id, c.Text, c.IsSystem, c.Type, c.UserId, c.CreatedAt)).ToList(),
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
