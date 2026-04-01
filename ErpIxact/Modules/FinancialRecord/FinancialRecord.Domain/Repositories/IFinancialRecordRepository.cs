using FinancialRecord.Domain.Enums;
using FinancialRecordEntity = FinancialRecord.Domain.Entities.FinancialRecord;

namespace FinancialRecord.Domain.Repositories;

public interface IFinancialRecordRepository
{
    Task<(IReadOnlyList<FinancialRecordEntity> Items, int TotalCount)> GetAllCurrentMonthAsync(
        int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<FinancialRecordEntity> Items, int TotalCount)> GetByMonthYearAsync(
        int month, int year, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<FinancialRecordEntity> Items, int TotalCount)> GetUpcomingDueAsync(
        int days, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<FinancialRecordEntity> Items, int TotalCount)> GetOverdueAsync(
        int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<FinancialRecordEntity> Items, int TotalCount)> GetByStatusAsync(
        FinancialRecordStatus status, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    Task<FinancialRecordEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(FinancialRecordEntity record, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<FinancialRecordEntity> records, CancellationToken cancellationToken = default);

    Task UpdateAsync(FinancialRecordEntity record, CancellationToken cancellationToken = default);

    Task UpdateOverdueStatusAsync(CancellationToken cancellationToken = default);
}
