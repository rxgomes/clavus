using BankAccount.Application.Commands.CreateBankAccount;
using BankAccount.Domain.Repositories;
using BankAccount.Infrastructure.Data;
using BankAccount.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BankAccountEntity = BankAccount.Domain.Entities.BankAccount;

namespace BankAccount.Tests.Integration;

public class BankAccountIntegrationTests : IDisposable
{
    private readonly BankAccountDbContext _context;
    private readonly IBankAccountRepository _repository;

    public BankAccountIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<BankAccountDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BankAccountDbContext(options);
        _repository = new BankAccountRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    // --- Busca: retornar todos ---

    [Fact]
    public async Task GetAllAsync_WhenAccountsExist_ShouldReturnAllAccounts()
    {
        var account1 = new BankAccountEntity("Banco do Brasil", "111111111", "1");
        var account2 = new BankAccountEntity("Caixa Econômica", "222222222", "2");

        await _repository.AddAsync(account1);
        await _repository.AddAsync(account2);

        var result = await _repository.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    // --- Busca: por NumberAccount + DigitAccount existente ---

    [Fact]
    public async Task GetByAccountAsync_WhenAccountExists_ShouldReturnAccount()
    {
        const string numberAccount = "123456789";
        const string digitAccount = "1";

        var account = new BankAccountEntity("Banco do Brasil", numberAccount, digitAccount);
        await _repository.AddAsync(account);

        var result = await _repository.GetByAccountAsync(numberAccount, digitAccount);

        Assert.NotNull(result);
        Assert.Equal(numberAccount, result.NumberAccount);
        Assert.Equal(digitAccount, result.DigitAccount);
    }

    // --- Busca: por NumberAccount + DigitAccount inexistente ---

    [Fact]
    public async Task GetByAccountAsync_WhenAccountDoesNotExist_ShouldReturnNull()
    {
        var result = await _repository.GetByAccountAsync("999999999", "9");

        Assert.Null(result);
    }

    // --- Exclusão lógica ---

    [Fact]
    public async Task DeactivateAsync_WhenAccountIsDeactivated_ShouldSetActiveToFalseAndKeepRecord()
    {
        var account = new BankAccountEntity("Banco do Brasil", "123456789", "1");
        await _repository.AddAsync(account);

        account.Deactivate();
        await _repository.DeactivateAsync(account);

        var allAccounts = await _repository.GetAllAsync();
        Assert.Single(allAccounts);

        var deactivatedAccount = allAccounts.First();
        Assert.False(deactivatedAccount.Active);
        Assert.Equal(account.Id, deactivatedAccount.Id);
    }

    // --- End-to-end: criação com sucesso via MediatR ---

    [Fact]
    public async Task CreateBankAccount_EndToEnd_ShouldPersistAndReturnSuccess()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CreateBankAccountCommand).Assembly));

        services.AddScoped<IBankAccountRepository>(_ => _repository);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var command = new CreateBankAccountCommand("Banco do Brasil", "123456789", "1");

        var result = await mediator.Send(command);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var allAccounts = await _repository.GetAllAsync();
        Assert.Single(allAccounts);
        Assert.Equal("123456789", allAccounts[0].NumberAccount);
        Assert.Equal("1", allAccounts[0].DigitAccount);
        Assert.True(allAccounts[0].Active);
    }
}
