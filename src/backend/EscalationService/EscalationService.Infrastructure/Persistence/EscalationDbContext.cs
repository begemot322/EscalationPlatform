using EscalationService.Application.Common.Interfaces;
using EscalationService.Domain.Entities;
using EscalationService.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace EscalationService.Infrastructure.Persistence;

/// <summary>
/// Контекст базы данных для сервиса эскалаций.
/// </summary>
public class EscalationDbContext : DbContext, IEscalationDbContext
{
    public EscalationDbContext(DbContextOptions<EscalationDbContext> options) : base(options) { }
    
    /// <summary>
    /// Эскалации.
    /// </summary>
    public virtual DbSet<Escalation> Escalations { get; set; } = null!;

    /// <summary>
    /// Проекты.
    /// </summary>
    public virtual DbSet<Project> Projects { get; set; } = null!;

    /// <summary>
    /// Политики SLA.
    /// </summary>
    public virtual DbSet<SlaPolicy> SlaPolicies { get; set; } = null!;

    /// <summary>
    /// Комментарии к эскалациям.
    /// </summary>
    public virtual DbSet<Comment> Comments { get; set; } = null!;

    /// <summary>
    /// Вложения (файлы).
    /// </summary>
    public virtual DbSet<Attachment> Attachments { get; set; } = null!;

    /// <summary>
    /// Критерии выполнения эскалаций.
    /// </summary>
    public virtual DbSet<Criterion> Criteria { get; set; } = null!;

    /// <summary>
    /// Метрики эскалаций.
    /// </summary>
    public virtual DbSet<EscalationMetrics> EscalationMetrics { get; set; } = null!;

    /// <summary>
    /// Участники эскалаций.
    /// </summary>
    public virtual DbSet<EscalationParticipant> EscalationParticipants { get; set; } = null!;

    /// <summary>
    /// Владельцы проектов.
    /// </summary>
    public virtual DbSet<ProjectOwner> ProjectOwners { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EscalationConfiguration());
        modelBuilder.ApplyConfiguration(new ProjectConfiguration());
        modelBuilder.ApplyConfiguration(new SlaPolicyConfiguration());
        modelBuilder.ApplyConfiguration(new CommentConfiguration());
        modelBuilder.ApplyConfiguration(new AttachmentConfiguration());
        modelBuilder.ApplyConfiguration(new CriterionConfiguration());
        modelBuilder.ApplyConfiguration(new EscalationMetricsConfiguration());
        modelBuilder.ApplyConfiguration(new EscalationParticipantConfiguration());
        modelBuilder.ApplyConfiguration(new ProjectOwnerConfiguration());
    }
}