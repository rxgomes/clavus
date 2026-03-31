using BankAccountEntity = BankAccount.Domain.Entities.BankAccount;

namespace BankAccount.Domain.Repositories;

public interface IBankAccountRepository
{
    Task<BankAccountEntity?> GetByAccountAsync(string numberAccount, string digitAccount, CancellationToken cancellationToken = default);
    Task<List<BankAccountEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(BankAccountEntity bankAccount, CancellationToken cancellationToken = default);
    Task DeactivateAsync(BankAccountEntity bankAccount, CancellationToken cancellationToken = default);
}
