using EscalationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EscalationService.Infrastructure.Persistence.Configurations;

public sealed class AttachmentConfiguration: IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.Property(a => a.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.StoragePath)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(a => a.EscalationId).IsRequired();

        builder.HasIndex(a => a.EscalationId);
    }
}