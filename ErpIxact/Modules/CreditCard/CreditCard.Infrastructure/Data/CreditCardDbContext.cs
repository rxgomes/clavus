using CreditCard.Domain.Entities;
using CreditCard.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using CreditCardEntity = CreditCard.Domain.Entities.CreditCard;

namespace CreditCard.Infrastructure.Data;

public class CreditCardDbContext : DbContext
{
    public CreditCardDbContext(DbContextOptions<CreditCardDbContext> options) : base(options) { }

    public DbSet<CreditCardEntity> CreditCards => Set<CreditCardEntity>();
    public DbSet<CardPurchase> CardPurchases => Set<CardPurchase>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CreditCardConfiguration());
        modelBuilder.ApplyConfiguration(new CardPurchaseConfiguration());
    }
}
