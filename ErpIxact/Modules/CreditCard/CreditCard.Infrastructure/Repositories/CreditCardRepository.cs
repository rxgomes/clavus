using CreditCard.Domain.Repositories;
using CreditCard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CreditCardEntity = CreditCard.Domain.Entities.CreditCard;

namespace CreditCard.Infrastructure.Repositories;

public class CreditCardRepository : ICreditCardRepository
{
    private readonly CreditCardDbContext _context;

    public CreditCardRepository(CreditCardDbContext context)
    {
        _context = context;
    }

    public async Task<CreditCardEntity?> GetByNameAndFlagAsync(string name, string flag, CancellationToken cancellationToken = default)
        => await _context.CreditCards
            .FirstOrDefaultAsync(c => c.Name == name && c.Flag == flag, cancellationToken);

    public async Task<List<CreditCardEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.CreditCards.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<List<CreditCardEntity>> GetActiveAsync(CancellationToken cancellationToken = default)
        => await _context.CreditCards.AsNoTracking().Where(c => c.Active).ToListAsync(cancellationToken);

    public async Task<CreditCardEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.CreditCards.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<List<CreditCardEntity>> GetByFlagAsync(string flag, CancellationToken cancellationToken = default)
        => await _context.CreditCards.AsNoTracking().Where(c => c.Flag == flag).ToListAsync(cancellationToken);

    public async Task<List<CreditCardEntity>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        => await _context.CreditCards.AsNoTracking().Where(c => c.Name == name).ToListAsync(cancellationToken);

    public async Task AddAsync(CreditCardEntity creditCard, CancellationToken cancellationToken = default)
    {
        await _context.CreditCards.AddAsync(creditCard, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
