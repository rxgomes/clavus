using Microsoft.EntityFrameworkCore;
using Patners.Domain.Entities;
using Patners.Domain.Repositories;
using Patners.Infrastructure.Data;
using Shared.Kernel.FunctionsString;

namespace Patners.Infrastructure.Repositories;

public class PartnersRepository : IPartnersRepository
{
    private readonly PartnersDbContext _context;

    public PartnersRepository(PartnersDbContext context)
    {
        _context = context;
    }

    public async Task<Partners?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Partners.FindAsync([id], cancellationToken);

    public async Task<Partners?> GetByDocNumberAsync(string docNumber, CancellationToken cancellationToken = default)
    {
        var digits = StringFunctions.ExtractDigits(docNumber);
        return await _context.Partners
            .AsNoTracking()
            .FirstOrDefaultAsync(p => EF.Property<string>(p, "_docNumber") == digits, cancellationToken);
    }
    
    public async Task<List<Partners>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Partners
            .AsNoTracking()
            .Where(p => EF.Functions.Like(p.Name, $"%{name}%"))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Partners>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Partners.AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(Partners partner, CancellationToken cancellationToken = default)
    {
        await _context.Partners.AddAsync(partner, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Partners partner, CancellationToken cancellationToken = default)
    {
        _context.Partners.Update(partner);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
