using BankAccount.Domain.Repositories;
using BankAccount.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BankAccountEntity = BankAccount.Domain.Entities.BankAccount;

namespace BankAccount.Infrastructure.Repositories;

public class BankAccountRepository : IBankAccountRepository
{
    private readonly BankAccountDbContext _context;

    public BankAccountRepository(BankAccountDbContext context)
    {
        _context = context;
    }

    public async Task<BankAccountEntity?> GetByAccountAsync(string numberAccount, string digitAccount, CancellationToken cancellationToken = default)
        => await _context.BankAccounts
            .FirstOrDefaultAsync(b => b.NumberAccount == numberAccount && b.DigitAccount == digitAccount, cancellationToken);

    public async Task<List<BankAccountEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.BankAccounts.AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(BankAccountEntity bankAccount, CancellationToken cancellationToken = default)
    {
        await _context.BankAccounts.AddAsync(bankAccount, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateAsync(BankAccountEntity bankAccount, CancellationToken cancellationToken = default)
    {
        _context.BankAccounts.Update(bankAccount);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
