using Microsoft.EntityFrameworkCore;
using FinancialRecordEntity = FinancialRecord.Domain.Entities.FinancialRecord;

namespace FinancialRecord.Infrastructure.Data;

public class FinancialRecordDbContext : DbContext
{
    public FinancialRecordDbContext(DbContextOptions<FinancialRecordDbContext> options) : base(options) { }

    public DbSet<FinancialRecordEntity> FinancialRecords => Set<FinancialRecordEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FinancialRecordEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Value).IsRequired();
            entity.Property(e => e.DueDate).IsRequired();
            entity.Property(e => e.TotalInstallment).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.Installment).IsRequired();
            entity.Property(e => e.Active).IsRequired();
        });
    }
}
