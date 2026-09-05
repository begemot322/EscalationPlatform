using EscalationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EscalationService.Infrastructure.Persistence.Configurations;

public sealed class CriterionConfiguration: IEntityTypeConfiguration<Criterion>
{
    public void Configure(EntityTypeBuilder<Criterion> builder)
    {
        builder.Property(c => c.Name)
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(c => c.Order).IsRequired();

        builder.Property(c => c.IsCompleted)
            .IsRequired();
        
        builder.Property(c => c.EscalationId).IsRequired();

        builder.HasIndex(c => c.EscalationId);
        builder.HasIndex(c => new { c.EscalationId, c.Order });
    }
}