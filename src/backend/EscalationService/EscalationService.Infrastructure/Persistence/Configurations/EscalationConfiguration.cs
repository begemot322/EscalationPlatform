using EscalationService.Domain.Entities;
using EscalationService.Domain.Enums;
using EscalationService.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EscalationService.Infrastructure.Persistence.Configurations;

public sealed class EscalationConfiguration : IEntityTypeConfiguration<Escalation>
{
    public void Configure(EntityTypeBuilder<Escalation> builder)
    {
        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasMaxLength(50)
            .HasConversion<SnakeCaseEnumConverter<EscalationStatus>>()
            .IsRequired();
        
        builder.Property(e => e.Priority)
            .HasMaxLength(50)
            .HasConversion<SnakeCaseEnumConverter<EscalationPriority>>()
            .IsRequired();
        
        builder.Property(e => e.Type)
            .HasMaxLength(50)
            .HasConversion<SnakeCaseEnumConverter<EscalationType>>()
            .IsRequired();
        
        builder.Property(e => e.AuthorId).IsRequired();
        builder.Property(e => e.SlaPolicyId).IsRequired();
        builder.Property(e => e.ProjectId).IsRequired();
        
        builder.HasOne(e => e.SlaPolicy)
            .WithMany()
            .HasForeignKey(e => e.SlaPolicyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Project)
            .WithMany()
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Metrics)
            .WithOne(m => m.Escalation)
            .HasForeignKey<EscalationMetrics>(m => m.EscalationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Participants)
            .WithOne(p => p.Escalation)
            .HasForeignKey(p => p.EscalationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Comments)
            .WithOne(c => c.Escalation)
            .HasForeignKey(c => c.EscalationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Criteria)
            .WithOne(c => c.Escalation)
            .HasForeignKey(c => c.EscalationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Attachments)
            .WithOne(a => a.Escalation)
            .HasForeignKey(a => a.EscalationId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.Priority);
        builder.HasIndex(e => e.AuthorId);
    }
}