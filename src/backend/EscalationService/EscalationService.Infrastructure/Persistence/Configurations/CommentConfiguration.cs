using EscalationService.Domain.Entities;
using EscalationService.Domain.Enums;
using EscalationService.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EscalationService.Infrastructure.Persistence.Configurations;

public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.Property(c => c.Text)
            .IsRequired();

        builder.Property(c => c.IsSystem)
            .IsRequired();
        
        builder.Property(c => c.Type)
            .HasMaxLength(50)
            .HasConversion<SnakeCaseEnumConverter<CommentType>>()
            .IsRequired();
        
        builder.Property(c => c.UserId).IsRequired();
        builder.Property(c => c.EscalationId).IsRequired();
    }
}