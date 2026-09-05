using EscalationService.Domain.Entities;
using EscalationService.Domain.Enums;
using EscalationService.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EscalationService.Infrastructure.Persistence.Configurations;

public sealed class SlaPolicyConfiguration: IEntityTypeConfiguration<SlaPolicy>
{
    public void Configure(EntityTypeBuilder<SlaPolicy> builder)
    {
        builder.Property(s => s.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.MinPriorityLevel)
            .HasMaxLength(50)
            .HasConversion<SnakeCaseEnumConverter<EscalationPriority>>()
            .IsRequired();

        builder.Property(s => s.IsActive)
            .IsRequired();

        builder.Property(s => s.ResponseTimeMinutes).IsRequired();
        builder.Property(s => s.ResolutionTimeMinutes).IsRequired();

        builder.HasIndex(s => s.IsActive);
        builder.HasIndex(s => s.MinPriorityLevel);
    }
}