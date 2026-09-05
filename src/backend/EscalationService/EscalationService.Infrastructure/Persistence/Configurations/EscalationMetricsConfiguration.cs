using EscalationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EscalationService.Infrastructure.Persistence.Configurations;

public sealed class EscalationMetricsConfiguration : IEntityTypeConfiguration<EscalationMetrics>
{
    public void Configure(EntityTypeBuilder<EscalationMetrics> builder)
    {
        builder.HasKey(m => m.EscalationId);

        builder.Property(m => m.SlaViolated)
            .IsRequired();
        
        builder.HasIndex(m => m.SlaViolated);
    }
}