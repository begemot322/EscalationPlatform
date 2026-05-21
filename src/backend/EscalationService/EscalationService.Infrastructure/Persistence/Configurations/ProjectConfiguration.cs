using EscalationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EscalationService.Infrastructure.Persistence.Configurations;

public sealed class ProjectConfiguration: IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(p => p.Code)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.HasMany(p => p.Owners)
            .WithOne(o => o.Project)
            .HasForeignKey(o => o.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.Code).IsUnique();
    }
}