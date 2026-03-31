using CreditCard.Domain.Repositories;
using CreditCard.Infrastructure.Data;
using CreditCard.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using CreditCardEntity = CreditCard.Domain.Entities.CreditCard;

namespace CreditCard.Tests.Integration;

public class CreditCardIntegrationTests : IDisposable
{
    private readonly CreditCardDbContext _context;
    private readonly ICreditCardRepository _repository;

    public CreditCardIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<CreditCardDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CreditCardDbContext(options);
        _repository = new CreditCardRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    // --- Consultar todos os cartões ---

    [Fact]
    public async Task GetAllAsync_WhenCardsExist_ShouldReturnAllCards()
    {
        var card1 = new CreditCardEntity("Cartão Pessoal", "Visa", 10, 20);
        var card2 = new CreditCardEntity("Cartão Empresa", "Mastercard", 15, 25);

        await _repository.AddAsync(card1);
        await _repository.AddAsync(card2);

        var result = await _repository.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoCardsExist_ShouldReturnEmptyList()
    {
        var result = await _repository.GetAllAsync();

        Assert.Empty(result);
    }

    // --- Consultar todos os cartões onde Active = true ---

    [Fact]
    public async Task GetActiveAsync_ShouldReturnOnlyActiveCards()
    {
        var activeCard = new CreditCardEntity("Cartão Ativo", "Visa", 10, 20);
        var inactiveCard = new CreditCardEntity("Cartão Inativo", "Mastercard", 15, 25);
        inactiveCard.Deactivate();

        await _repository.AddAsync(activeCard);
        await _repository.AddAsync(inactiveCard);

        var result = await _repository.GetActiveAsync();

        Assert.Single(result);
        Assert.True(result[0].Active);
        Assert.Equal("Cartão Ativo", result[0].Name);
    }

    [Fact]
    public async Task GetActiveAsync_WhenAllCardsAreActive_ShouldReturnAll()
    {
        var card1 = new CreditCardEntity("Cartão Um", "Visa", 5, 10);
        var card2 = new CreditCardEntity("Cartão Dois", "Elo", 10, 15);

        await _repository.AddAsync(card1);
        await _repository.AddAsync(card2);

        var result = await _repository.GetActiveAsync();

        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.True(c.Active));
    }

    // --- Consultar cartão por Id ---

    [Fact]
    public async Task GetByIdAsync_WhenCardExists_ShouldReturnCard()
    {
        var card = new CreditCardEntity("Cartão Pessoal", "Visa", 10, 20);
        await _repository.AddAsync(card);

        var result = await _repository.GetByIdAsync(card.Id);

        Assert.NotNull(result);
        Assert.Equal(card.Id, result.Id);
        Assert.Equal("Cartão Pessoal", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCardDoesNotExist_ShouldReturnNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    // --- Consultar cartão por Flag ---

    [Fact]
    public async Task GetByFlagAsync_WhenCardsWithFlagExist_ShouldReturnThem()
    {
        var visaCard1 = new CreditCardEntity("Cartão Pessoal", "Visa", 10, 20);
        var visaCard2 = new CreditCardEntity("Cartão Empresa", "Visa", 15, 25);
        var mastercardCard = new CreditCardEntity("Cartão Outro", "Mastercard", 5, 10);

        await _repository.AddAsync(visaCard1);
        await _repository.AddAsync(visaCard2);
        await _repository.AddAsync(mastercardCard);

        var result = await _repository.GetByFlagAsync("Visa");

        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal("Visa", c.Flag));
    }

    [Fact]
    public async Task GetByFlagAsync_WhenNoCardMatchesFlag_ShouldReturnEmptyList()
    {
        var card = new CreditCardEntity("Cartão Pessoal", "Visa", 10, 20);
        await _repository.AddAsync(card);

        var result = await _repository.GetByFlagAsync("Amex");

        Assert.Empty(result);
    }

    // --- Consultar cartão por Nome ---

    [Fact]
    public async Task GetByNameAsync_WhenCardWithNameExists_ShouldReturnIt()
    {
        var card = new CreditCardEntity("Cartão Pessoal", "Visa", 10, 20);
        await _repository.AddAsync(card);

        var result = await _repository.GetByNameAsync("Cartão Pessoal");

        Assert.Single(result);
        Assert.Equal("Cartão Pessoal", result[0].Name);
    }

    [Fact]
    public async Task GetByNameAsync_WhenNoCardMatchesName_ShouldReturnEmptyList()
    {
        var card = new CreditCardEntity("Cartão Pessoal", "Visa", 10, 20);
        await _repository.AddAsync(card);

        var result = await _repository.GetByNameAsync("Nome Inexistente");

        Assert.Empty(result);
    }

    // --- Consultar cartão por Flag e Name ---

    [Fact]
    public async Task GetByNameAndFlagAsync_WhenCardExists_ShouldReturnCard()
    {
        const string name = "Cartão Pessoal";
        const string flag = "Visa";

        var card = new CreditCardEntity(name, flag, 10, 20);
        await _repository.AddAsync(card);

        var result = await _repository.GetByNameAndFlagAsync(name, flag);

        Assert.NotNull(result);
        Assert.Equal(name, result.Name);
        Assert.Equal(flag, result.Flag);
    }

    [Fact]
    public async Task GetByNameAndFlagAsync_WhenOnlyNameMatches_ShouldReturnNull()
    {
        var card = new CreditCardEntity("Cartão Pessoal", "Visa", 10, 20);
        await _repository.AddAsync(card);

        var result = await _repository.GetByNameAndFlagAsync("Cartão Pessoal", "Mastercard");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByNameAndFlagAsync_WhenOnlyFlagMatches_ShouldReturnNull()
    {
        var card = new CreditCardEntity("Cartão Pessoal", "Visa", 10, 20);
        await _repository.AddAsync(card);

        var result = await _repository.GetByNameAndFlagAsync("Outro Nome", "Visa");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByNameAndFlagAsync_WhenCardDoesNotExist_ShouldReturnNull()
    {
        var result = await _repository.GetByNameAndFlagAsync("Nome Inexistente", "Flag Inexistente");

        Assert.Null(result);
    }
}
