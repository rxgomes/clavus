using FinancialRecord.Application.Commands.CreateFinancialRecord;
using FinancialRecord.Domain.Enums;
using FinancialRecord.Domain.Repositories;
using FinancialRecord.Infrastructure.Data;
using FinancialRecord.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialRecord.Tests.Integration;

public class FinancialRecordRepositoryIntegrationTests : IDisposable
{
    private readonly FinancialRecordDbContext _context;
    private readonly IFinancialRecordRepository _repository;

    public FinancialRecordRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<FinancialRecordDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new FinancialRecordDbContext(options);
        _repository = new FinancialRecordRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static FinancialRecordEntity CreateRecord(
        string description = "Conta teste",
        decimal value = 100.00m,
        DateOnly? dueDate = null,
        FinancialRecordStatus status = FinancialRecordStatus.Pending,
        int totalInstallment = 1,
        int installment = 1)
    {
        dueDate ??= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));
        return new FinancialRecordEntity(description, value, dueDate.Value, totalInstallment, status, installment);
    }

    // -------------------------------------------------------------------------
    // GetAllCurrentMonthAsync — retorna registros do mês corrente, Active = true, paginado
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAllCurrentMonthAsync_ShouldReturnOnlyCurrentMonthActiveRecords()
    {
        var today = DateTime.UtcNow;
        var currentMonthDueDate = DateOnly.FromDateTime(today);
        var nextMonthDueDate = DateOnly.FromDateTime(today.AddMonths(1));

        var recordCurrentMonth = CreateRecord(dueDate: currentMonthDueDate);
        var recordNextMonth = CreateRecord(dueDate: nextMonthDueDate);

        await _repository.AddAsync(recordCurrentMonth);
        await _repository.AddAsync(recordNextMonth);

        var (items, totalCount) = await _repository.GetAllCurrentMonthAsync(page: 1, pageSize: 50);

        Assert.Equal(1, totalCount);
        Assert.Single(items);
        Assert.Equal(currentMonthDueDate, items[0].DueDate);
    }

    [Fact]
    public async Task GetAllCurrentMonthAsync_ShouldNotReturnInactiveRecords()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var record = CreateRecord(dueDate: today);

        await _repository.AddAsync(record);
        record.Deactivate();
        await _repository.UpdateAsync(record);

        var (items, totalCount) = await _repository.GetAllCurrentMonthAsync(page: 1, pageSize: 50);

        Assert.Equal(0, totalCount);
        Assert.Empty(items);
    }

    [Fact]
    public async Task GetAllCurrentMonthAsync_ShouldRespectPageSize()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        for (var i = 0; i < 5; i++)
        {
            await _repository.AddAsync(CreateRecord(description: $"Conta {i}", dueDate: today));
        }

        var (items, totalCount) = await _repository.GetAllCurrentMonthAsync(page: 1, pageSize: 3);

        Assert.Equal(5, totalCount);
        Assert.Equal(3, items.Count);
    }

    [Fact]
    public async Task GetAllCurrentMonthAsync_DefaultPageSizeShouldBe50()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        for (var i = 0; i < 60; i++)
        {
            await _repository.AddAsync(CreateRecord(description: $"Conta {i}", dueDate: today));
        }

        var (items, totalCount) = await _repository.GetAllCurrentMonthAsync(page: 1);

        Assert.Equal(60, totalCount);
        Assert.Equal(50, items.Count);
    }

    [Fact]
    public async Task GetAllCurrentMonthAsync_ShouldSupportPagination()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        for (var i = 0; i < 5; i++)
        {
            await _repository.AddAsync(CreateRecord(description: $"Conta {i}", dueDate: today));
        }

        var (itemsPage1, _) = await _repository.GetAllCurrentMonthAsync(page: 1, pageSize: 3);
        var (itemsPage2, _) = await _repository.GetAllCurrentMonthAsync(page: 2, pageSize: 3);

        Assert.Equal(3, itemsPage1.Count);
        Assert.Equal(2, itemsPage2.Count);

        var allIds = itemsPage1.Select(r => r.Id).Concat(itemsPage2.Select(r => r.Id)).Distinct();
        Assert.Equal(5, allIds.Count());
    }

    // -------------------------------------------------------------------------
    // GetByMonthYearAsync — retorna registros do mês/ano informado, Active = true, paginado
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByMonthYearAsync_ShouldReturnOnlyRecordsForSpecifiedMonthAndYear()
    {
        var january2027 = new DateOnly(2027, 1, 15);
        var february2027 = new DateOnly(2027, 2, 15);

        var recordJan = CreateRecord(dueDate: january2027);
        var recordFeb = CreateRecord(dueDate: february2027);

        await _repository.AddAsync(recordJan);
        await _repository.AddAsync(recordFeb);

        var (items, totalCount) = await _repository.GetByMonthYearAsync(month: 1, year: 2027, page: 1, pageSize: 50);

        Assert.Equal(1, totalCount);
        Assert.Single(items);
        Assert.Equal(january2027, items[0].DueDate);
    }

    [Fact]
    public async Task GetByMonthYearAsync_ShouldNotReturnInactiveRecords()
    {
        var targetDate = new DateOnly(2027, 3, 10);
        var record = CreateRecord(dueDate: targetDate);

        await _repository.AddAsync(record);
        record.Deactivate();
        await _repository.UpdateAsync(record);

        var (items, totalCount) = await _repository.GetByMonthYearAsync(month: 3, year: 2027, page: 1, pageSize: 50);

        Assert.Equal(0, totalCount);
        Assert.Empty(items);
    }

    [Fact]
    public async Task GetByMonthYearAsync_DefaultPageSizeShouldBe50()
    {
        var targetDate = new DateOnly(2027, 6, 10);

        for (var i = 0; i < 60; i++)
        {
            await _repository.AddAsync(CreateRecord(description: $"Conta {i}", dueDate: targetDate));
        }

        var (items, totalCount) = await _repository.GetByMonthYearAsync(month: 6, year: 2027, page: 1);

        Assert.Equal(60, totalCount);
        Assert.Equal(50, items.Count);
    }

    [Fact]
    public async Task GetByMonthYearAsync_ShouldSupportCustomPageSize()
    {
        var targetDate = new DateOnly(2027, 4, 10);

        for (var i = 0; i < 10; i++)
        {
            await _repository.AddAsync(CreateRecord(description: $"Conta {i}", dueDate: targetDate));
        }

        var (items, totalCount) = await _repository.GetByMonthYearAsync(month: 4, year: 2027, page: 1, pageSize: 5);

        Assert.Equal(10, totalCount);
        Assert.Equal(5, items.Count);
    }

    // -------------------------------------------------------------------------
    // GetUpcomingDueAsync — vencimento nos próximos N dias, Active = true, paginado
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetUpcomingDueAsync_ShouldReturnRecordsDueWithinSpecifiedDays()
    {
        var inThreeDays = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3));
        var inTenDays = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));

        var recordSoon = CreateRecord(dueDate: inThreeDays);
        var recordLater = CreateRecord(dueDate: inTenDays);

        await _repository.AddAsync(recordSoon);
        await _repository.AddAsync(recordLater);

        var (items, totalCount) = await _repository.GetUpcomingDueAsync(days: 5, page: 1, pageSize: 50);

        Assert.Equal(1, totalCount);
        Assert.Single(items);
        Assert.Equal(inThreeDays, items[0].DueDate);
    }

    [Fact]
    public async Task GetUpcomingDueAsync_ShouldNotReturnInactiveRecords()
    {
        var inTwoDays = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));
        var record = CreateRecord(dueDate: inTwoDays);

        await _repository.AddAsync(record);
        record.Deactivate();
        await _repository.UpdateAsync(record);

        var (items, totalCount) = await _repository.GetUpcomingDueAsync(days: 5, page: 1, pageSize: 50);

        Assert.Equal(0, totalCount);
        Assert.Empty(items);
    }

    [Fact]
    public async Task GetUpcomingDueAsync_ShouldIncludeRecordsDueToday()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var record = CreateRecord(dueDate: today);

        await _repository.AddAsync(record);

        var (items, totalCount) = await _repository.GetUpcomingDueAsync(days: 0, page: 1, pageSize: 50);

        Assert.Equal(1, totalCount);
        Assert.Single(items);
    }

    [Fact]
    public async Task GetUpcomingDueAsync_DefaultPageSizeShouldBe50()
    {
        var inOneDays = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        for (var i = 0; i < 60; i++)
        {
            await _repository.AddAsync(CreateRecord(description: $"Conta {i}", dueDate: inOneDays));
        }

        var (items, totalCount) = await _repository.GetUpcomingDueAsync(days: 7, page: 1);

        Assert.Equal(60, totalCount);
        Assert.Equal(50, items.Count);
    }

    // -------------------------------------------------------------------------
    // GetOverdueAsync — Status = Vencido, Active = true, paginado
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetOverdueAsync_ShouldReturnOnlyOverdueRecords()
    {
        var dueDateFuture = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));

        var overdueRecord = CreateRecord(status: FinancialRecordStatus.Overdue, dueDate: dueDateFuture);
        var pendingRecord = CreateRecord(status: FinancialRecordStatus.Pending, dueDate: dueDateFuture);
        var paidRecord = CreateRecord(status: FinancialRecordStatus.Paid, dueDate: dueDateFuture);

        await _repository.AddAsync(overdueRecord);
        await _repository.AddAsync(pendingRecord);
        await _repository.AddAsync(paidRecord);

        var (items, totalCount) = await _repository.GetOverdueAsync(page: 1, pageSize: 50);

        Assert.Equal(1, totalCount);
        Assert.Single(items);
        Assert.Equal(FinancialRecordStatus.Overdue, items[0].Status);
    }

    [Fact]
    public async Task GetOverdueAsync_ShouldNotReturnInactiveRecords()
    {
        var dueDateFuture = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));
        var record = CreateRecord(status: FinancialRecordStatus.Overdue, dueDate: dueDateFuture);

        await _repository.AddAsync(record);
        record.Deactivate();
        await _repository.UpdateAsync(record);

        var (items, totalCount) = await _repository.GetOverdueAsync(page: 1, pageSize: 50);

        Assert.Equal(0, totalCount);
        Assert.Empty(items);
    }

    [Fact]
    public async Task GetOverdueAsync_DefaultPageSizeShouldBe50()
    {
        var dueDateFuture = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));

        for (var i = 0; i < 60; i++)
        {
            await _repository.AddAsync(CreateRecord(description: $"Conta {i}", status: FinancialRecordStatus.Overdue, dueDate: dueDateFuture));
        }

        var (items, totalCount) = await _repository.GetOverdueAsync(page: 1);

        Assert.Equal(60, totalCount);
        Assert.Equal(50, items.Count);
    }

    // -------------------------------------------------------------------------
    // GetByStatusAsync — filtra por status informado, Active = true, paginado
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(FinancialRecordStatus.Pending)]
    [InlineData(FinancialRecordStatus.Overdue)]
    [InlineData(FinancialRecordStatus.Paid)]
    public async Task GetByStatusAsync_ShouldReturnOnlyRecordsWithSpecifiedStatus(FinancialRecordStatus targetStatus)
    {
        var dueDateFuture = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));

        var pending = CreateRecord(status: FinancialRecordStatus.Pending, dueDate: dueDateFuture);
        var overdue = CreateRecord(status: FinancialRecordStatus.Overdue, dueDate: dueDateFuture);
        var paid = CreateRecord(status: FinancialRecordStatus.Paid, dueDate: dueDateFuture);

        await _repository.AddAsync(pending);
        await _repository.AddAsync(overdue);
        await _repository.AddAsync(paid);

        var (items, totalCount) = await _repository.GetByStatusAsync(targetStatus, page: 1, pageSize: 50);

        Assert.Equal(1, totalCount);
        Assert.Single(items);
        Assert.Equal(targetStatus, items[0].Status);
    }

    [Fact]
    public async Task GetByStatusAsync_ShouldNotReturnInactiveRecords()
    {
        var dueDateFuture = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));
        var record = CreateRecord(status: FinancialRecordStatus.Pending, dueDate: dueDateFuture);

        await _repository.AddAsync(record);
        record.Deactivate();
        await _repository.UpdateAsync(record);

        var (items, totalCount) = await _repository.GetByStatusAsync(FinancialRecordStatus.Pending, page: 1, pageSize: 50);

        Assert.Equal(0, totalCount);
        Assert.Empty(items);
    }

    [Fact]
    public async Task GetByStatusAsync_DefaultPageSizeShouldBe50()
    {
        var dueDateFuture = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));

        for (var i = 0; i < 60; i++)
        {
            await _repository.AddAsync(CreateRecord(description: $"Conta {i}", status: FinancialRecordStatus.Pending, dueDate: dueDateFuture));
        }

        var (items, totalCount) = await _repository.GetByStatusAsync(FinancialRecordStatus.Pending, page: 1);

        Assert.Equal(60, totalCount);
        Assert.Equal(50, items.Count);
    }

    [Fact]
    public async Task GetByStatusAsync_ShouldSupportCustomPageSize()
    {
        var dueDateFuture = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));

        for (var i = 0; i < 10; i++)
        {
            await _repository.AddAsync(CreateRecord(description: $"Conta {i}", status: FinancialRecordStatus.Pending, dueDate: dueDateFuture));
        }

        var (items, totalCount) = await _repository.GetByStatusAsync(FinancialRecordStatus.Pending, page: 1, pageSize: 4);

        Assert.Equal(10, totalCount);
        Assert.Equal(4, items.Count);
    }

    // -------------------------------------------------------------------------
    // UpdateOverdueStatusAsync — marca como Vencido registros com DueDate < hoje
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateOverdueStatusAsync_ShouldMarkPastDueRecordsAsOverdue()
    {
        // Para criar registros com DueDate no passado, usamos o construtor protegido
        // via reflection ou um helper de teste — aqui simulamos via AddAsync direto no context
        var pastRecord = FinancialRecordTestBuilder.CreateWithPastDueDate(FinancialRecordStatus.Pending);

        await _context.FinancialRecords.AddAsync(pastRecord);
        await _context.SaveChangesAsync();

        await _repository.UpdateOverdueStatusAsync();

        var updated = await _context.FinancialRecords.FindAsync(pastRecord.Id);
        Assert.Equal(FinancialRecordStatus.Overdue, updated!.Status);
    }

    [Fact]
    public async Task UpdateOverdueStatusAsync_ShouldNotMarkFutureDueRecordsAsOverdue()
    {
        var futureRecord = CreateRecord(status: FinancialRecordStatus.Pending);
        await _repository.AddAsync(futureRecord);

        await _repository.UpdateOverdueStatusAsync();

        var unchanged = await _context.FinancialRecords.FindAsync(futureRecord.Id);
        Assert.Equal(FinancialRecordStatus.Pending, unchanged!.Status);
    }

    [Fact]
    public async Task UpdateOverdueStatusAsync_ShouldNotAffectAlreadyPaidRecords()
    {
        var pastPaidRecord = FinancialRecordTestBuilder.CreateWithPastDueDate(FinancialRecordStatus.Paid);

        await _context.FinancialRecords.AddAsync(pastPaidRecord);
        await _context.SaveChangesAsync();

        await _repository.UpdateOverdueStatusAsync();

        var unchanged = await _context.FinancialRecords.FindAsync(pastPaidRecord.Id);
        Assert.Equal(FinancialRecordStatus.Paid, unchanged!.Status);
    }

    // -------------------------------------------------------------------------
    // Exclusão lógica — Active = false, registro permanece no banco
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Deactivate_ShouldSetActiveToFalseAndKeepRecord()
    {
        var record = CreateRecord();
        await _repository.AddAsync(record);

        record.Deactivate();
        await _repository.UpdateAsync(record);

        var allRecords = await _context.FinancialRecords.ToListAsync();
        Assert.Single(allRecords);
        Assert.False(allRecords[0].Active);
        Assert.Equal(record.Id, allRecords[0].Id);
    }

    [Fact]
    public async Task Deactivate_ShouldNotBeReturnedByActiveOnlyQueries()
    {
        var record = CreateRecord();
        await _repository.AddAsync(record);

        record.Deactivate();
        await _repository.UpdateAsync(record);

        var (items, _) = await _repository.GetAllCurrentMonthAsync(page: 1, pageSize: 50);
        Assert.Empty(items);
    }

    // -------------------------------------------------------------------------
    // End-to-end — criação com sucesso via MediatR
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateFinancialRecord_EndToEnd_ShouldPersistAndReturnSuccess()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CreateFinancialRecordCommand).Assembly));

        services.AddScoped<IFinancialRecordRepository>(_ => _repository);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var dueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));

        var command = new CreateFinancialRecordCommand(
            Description: "Conta de gás",
            Value: 120.00m,
            DueDate: dueDate,
            TotalInstallment: 1,
            Status: FinancialRecordStatus.Pending);

        var result = await mediator.Send(command);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value);

        var allRecords = await _context.FinancialRecords.ToListAsync();
        Assert.Single(allRecords);
        Assert.Equal("Conta de gás", allRecords[0].Description);
        Assert.Equal(120.00m, allRecords[0].Value);
        Assert.Equal(dueDate, allRecords[0].DueDate);
        Assert.True(allRecords[0].Active);
    }
}

// -------------------------------------------------------------------------
// Helper para criar entidades com DueDate no passado (contornando a validação
// do construtor público via reflection — necessário apenas em testes de repositório)
// -------------------------------------------------------------------------

internal static class FinancialRecordTestBuilder
{
    /// <summary>
    /// Cria a entidade com uma data futura válida e depois substitui a DueDate
    /// via reflection, simulando registros já persistidos com DueDate no passado.
    /// Não requer nenhum método especial no código de produção.
    /// </summary>
    public static FinancialRecordEntity CreateWithPastDueDate(
        FinancialRecordStatus status = FinancialRecordStatus.Pending)
    {
        var record = new FinancialRecordEntity(
            "Conta vencida", 100.00m,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            totalInstallment: 1, status);

        var property = typeof(FinancialRecordEntity)
            .GetProperty(nameof(FinancialRecordEntity.DueDate))!;
        property.SetValue(record, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));

        return record;
    }
}
