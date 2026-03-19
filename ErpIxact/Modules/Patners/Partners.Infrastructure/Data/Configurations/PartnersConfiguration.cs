using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Patners.Domain.Entities;
using Shared.Kernel.ValueObjects;

namespace Patners.Infrastructure.Data.Configurations;

public class PartnersConfiguration : IEntityTypeConfiguration<Partners>
{
    public void Configure(EntityTypeBuilder<Partners> builder)
    {
        builder.ToTable("partners");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .IsRequired();

        builder.Property(p => p.DocNumber)
            .HasConversion(
                v => v.Value,
                v => new DocNumber(v))
            .HasColumnName("doc_number")
            .HasMaxLength(14)
            .IsRequired();

        builder.HasIndex(p => p.DocNumber)
            .IsUnique();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(p => p.Active)
            .HasColumnName("active")
            .IsRequired();

        builder.Ignore(p => p.DomainEvents);
    }
}
