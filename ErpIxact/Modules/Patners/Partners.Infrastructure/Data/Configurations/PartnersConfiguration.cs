using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Patners.Domain.Entities;

namespace Patners.Infrastructure.Data.Configurations;

public class PartnersConfiguration : IEntityTypeConfiguration<Partners>
{
    public void Configure(EntityTypeBuilder<Partners> builder)
    {
        builder.ToTable("partners");

        builder.HasKey(p => p.Id);

        builder.Ignore(p => p.DocNumber);

        builder.Property<string>("_docNumber")
            .HasField("_docNumber")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("doc_number")
            .HasMaxLength(14)
            .IsRequired();

        builder.HasIndex("_docNumber")
            .HasDatabaseName("ix_partners_doc_number")
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
