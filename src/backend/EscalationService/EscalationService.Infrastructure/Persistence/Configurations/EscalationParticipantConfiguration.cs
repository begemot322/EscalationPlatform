using EscalationService.Domain.Entities;
using EscalationService.Domain.Enums;
using EscalationService.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EscalationService.Infrastructure.Persistence.Configurations;

public sealed class EscalationParticipantConfiguration : IEntityTypeConfiguration<EscalationParticipant>
{
    public void Configure(EntityTypeBuilder<EscalationParticipant> builder)
    {
        builder.HasKey(p => new { p.EscalationId, p.UserId });
        
        builder.Property(p => p.Role)
            .HasMaxLength(50)
            .HasConversion<SnakeCaseEnumConverter<ParticipantRole>>()
            .IsRequired();
        
        builder.Property(p => p.AssignedAt).IsRequired();

        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.Role);
    }
}