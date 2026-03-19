using Microsoft.EntityFrameworkCore;
using Patners.Domain.Entities;

namespace Patners.Infrastructure.Data;

public class PartnersDbContext : DbContext
{
    public PartnersDbContext(DbContextOptions<PartnersDbContext> options) : base(options) { }

    public DbSet<Partners> Partners => Set<Partners>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PartnersDbContext).Assembly);
    }
}
