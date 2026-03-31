using BankAccount.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using BankAccountEntity = BankAccount.Domain.Entities.BankAccount;

namespace BankAccount.Infrastructure.Data;

public class BankAccountDbContext : DbContext
{
    public BankAccountDbContext(DbContextOptions<BankAccountDbContext> options) : base(options) { }

    public DbSet<BankAccountEntity> BankAccounts => Set<BankAccountEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new BankAccountConfiguration());
    }
}
