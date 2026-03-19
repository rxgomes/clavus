using Patners.Domain.Entities;

namespace Patners.Domain.Repositories;

public interface IPartnersRepository
{
    Task<Partners?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Partners?> GetByDocNumberAsync(string docNumber, CancellationToken cancellationToken = default);
    Task<List<Partners>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Partners partner, CancellationToken cancellationToken = default);
    Task UpdateAsync(Partners partner, CancellationToken cancellationToken = default);
}