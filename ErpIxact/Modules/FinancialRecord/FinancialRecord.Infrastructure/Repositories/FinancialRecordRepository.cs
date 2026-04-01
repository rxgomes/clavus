using FinancialRecord.Domain.Enums;
using FinancialRecord.Domain.Repositories;
using FinancialRecord.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FinancialRecordEntity = FinancialRecord.Domain.Entities.FinancialRecord;

namespace FinancialRecord.Infrastructure.Repositories;

public class FinancialRecordRepository : IFinancialRecordRepository
{
    private readonly FinancialRecordDbContext _context;

    public FinancialRecordRepository(FinancialRecordDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<FinancialRecordEntity> Items, int TotalCount)> GetAllCurrentMonthAsync(
        int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow;
        var query = _context.FinancialRecords
            .Where(r => r.Active && r.DueDate.Month == today.Month && r.DueDate.Year == today.Year);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<FinancialRecordEntity> Items, int TotalCount)> GetByMonthYearAsync(
        int month, int year, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var query = _context.FinancialRecords
            .Where(r => r.Active && r.DueDate.Month == month && r.DueDate.Year == year);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<FinancialRecordEntity> Items, int TotalCount)> GetUpcomingDueAsync(
        int days, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var upcomingDate = today.AddDays(days);

        var query = _context.FinancialRecords
            .Where(r => r.Active && r.DueDate >= today && r.DueDate <= upcomingDate);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<FinancialRecordEntity> Items, int TotalCount)> GetOverdueAsync(
        int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var query = _context.FinancialRecords
            .Where(r => r.Active && r.Status == FinancialRecordStatus.Overdue);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<FinancialRecordEntity> Items, int TotalCount)> GetByStatusAsync(
        FinancialRecordStatus status, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var query = _context.FinancialRecords
            .Where(r => r.Active && r.Status == status);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<FinancialRecordEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.FinancialRecords.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task AddAsync(FinancialRecordEntity record, CancellationToken cancellationToken = default)
    {
        await _context.FinancialRecords.AddAsync(record, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<FinancialRecordEntity> records, CancellationToken cancellationToken = default)
    {
        await _context.FinancialRecords.AddRangeAsync(records, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(FinancialRecordEntity record, CancellationToken cancellationToken = default)
    {
        _context.FinancialRecords.Update(record);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateOverdueStatusAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var overdueRecords = await _context.FinancialRecords
            .Where(r => r.Active && r.Status == FinancialRecordStatus.Pending && r.DueDate < today)
            .ToListAsync(cancellationToken);

        foreach (var record in overdueRecords)
        {
            record.MarkAsOverdue();
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
