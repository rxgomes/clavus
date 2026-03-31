using CreditCardEntity = CreditCard.Domain.Entities.CreditCard;

namespace CreditCard.Domain.Repositories;

public interface ICreditCardRepository
{
    Task<CreditCardEntity?> GetByNameAndFlagAsync(string name, string flag, CancellationToken cancellationToken = default);
    Task<List<CreditCardEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<CreditCardEntity>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<CreditCardEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<CreditCardEntity>> GetByFlagAsync(string flag, CancellationToken cancellationToken = default);
    Task<List<CreditCardEntity>> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(CreditCardEntity creditCard, CancellationToken cancellationToken = default);
}
