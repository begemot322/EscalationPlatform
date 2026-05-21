using EscalationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EscalationService.Application.Common.Interfaces;

/// <summary>
/// Интерфейс контекста базы данных для сервиса эскалаций
/// </summary>
public interface IEscalationDbContext
{
    DbSet<Escalation> Escalations { get; }
    DbSet<Project> Projects { get; }
    DbSet<SlaPolicy> SlaPolicies { get; }
    DbSet<Comment> Comments { get; }
    DbSet<Attachment> Attachments { get; }
    DbSet<Criterion> Criteria { get; }
    DbSet<EscalationMetrics> EscalationMetrics { get; }
    DbSet<EscalationParticipant> EscalationParticipants { get; }
    DbSet<ProjectOwner> ProjectOwners { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}