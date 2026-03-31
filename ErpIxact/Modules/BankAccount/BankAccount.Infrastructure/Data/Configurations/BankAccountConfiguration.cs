using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BankAccountEntity = BankAccount.Domain.Entities.BankAccount;

namespace BankAccount.Infrastructure.Data.Configurations;

public class BankAccountConfiguration : IEntityTypeConfiguration<BankAccountEntity>
{
    public void Configure(EntityTypeBuilder<BankAccountEntity> builder)
    {
        builder.ToTable("bank_accounts");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.NameBank)
            .HasColumnName("name_bank")
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(b => b.NumberAccount)
            .HasColumnName("number_account")
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(b => b.DigitAccount)
            .HasColumnName("digit_account")
            .HasMaxLength(5)
            .IsRequired();

        builder.Property(b => b.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp without time zone")
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp without time zone");

        builder.Property(b => b.Active)
            .HasColumnName("active")
            .IsRequired();

        builder.Ignore(b => b.DomainEvents);
    }
}
