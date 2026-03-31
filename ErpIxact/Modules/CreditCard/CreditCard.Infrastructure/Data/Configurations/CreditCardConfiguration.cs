using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CreditCardEntity = CreditCard.Domain.Entities.CreditCard;

namespace CreditCard.Infrastructure.Data.Configurations;

public class CreditCardConfiguration : IEntityTypeConfiguration<CreditCardEntity>
{
    public void Configure(EntityTypeBuilder<CreditCardEntity> builder)
    {
        builder.ToTable("credit_cards");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(c => c.Flag)
            .HasColumnName("flag")
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(c => c.CloseDay)
            .HasColumnName("close_day")
            .IsRequired();

        builder.Property(c => c.DueDay)
            .HasColumnName("due_day")
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp without time zone")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp without time zone");

        builder.Property(c => c.Active)
            .HasColumnName("active")
            .IsRequired();

        builder.Ignore(c => c.DomainEvents);
    }
}
