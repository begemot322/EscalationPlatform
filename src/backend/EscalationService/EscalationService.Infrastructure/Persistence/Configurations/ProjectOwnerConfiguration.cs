using EscalationService.Domain.Entities;
using EscalationService.Domain.Enums;
using EscalationService.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EscalationService.Infrastructure.Persistence.Configurations;

public sealed class ProjectOwnerConfiguration  : IEntityTypeConfiguration<ProjectOwner>
{
    public void Configure(EntityTypeBuilder<ProjectOwner> builder)
    {
        builder.HasKey(o => new { o.ProjectId, o.UserId });
        
        builder.Property(c => c.Role)
            .HasMaxLength(50)
            .HasConversion<SnakeCaseEnumConverter<ProjectRole>>()
            .IsRequired();
        
        builder.Property(o => o.AssignedAt).IsRequired();

        builder.HasIndex(o => o.UserId);
    }
}