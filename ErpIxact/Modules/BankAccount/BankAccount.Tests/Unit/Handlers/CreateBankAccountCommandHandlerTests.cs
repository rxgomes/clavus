using BankAccount.Application.Commands.CreateBankAccount;
using BankAccount.Domain.Messages;
using BankAccount.Domain.Repositories;
using Moq;
using Shared.Kernel;
using Xunit;
using BankAccountEntity = BankAccount.Domain.Entities.BankAccount;

namespace BankAccount.Tests.Unit.Handlers;

public class CreateBankAccountCommandHandlerTests
{
    private readonly Mock<IBankAccountRepository> _repositoryMock;
    private readonly CreateBankAccountCommandHandler _handler;

    private const string ValidNameBank = "Banco do Brasil";
    private const string ValidNumberAccount = "123456789";
    private const string ValidDigitAccount = "1";

    public CreateBankAccountCommandHandlerTests()
    {
        _repositoryMock = new Mock<IBankAccountRepository>();
        _handler = new CreateBankAccountCommandHandler(_repositoryMock.Object);
    }

    // --- Verificação de duplicidade ---

    [Fact]
    public async Task Handle_WhenAccountAlreadyExistsAndIsActive_ShouldReturnConflictWithActiveMessage()
    {
        var existingAccount = new BankAccountEntity(ValidNameBank, ValidNumberAccount, ValidDigitAccount);

        _repositoryMock
            .Setup(r => r.GetByAccountAsync(ValidNumberAccount, ValidDigitAccount, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        var command = new CreateBankAccountCommand(ValidNameBank, ValidNumberAccount, ValidDigitAccount);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.Equal(BankAccountMessages.Errors.AlreadyExistsActive, result.Error);
    }

    [Fact]
    public async Task Handle_WhenAccountAlreadyExistsAndIsInactive_ShouldReturnConflictWithInactiveMessage()
    {
        var inactiveAccount = new BankAccountEntity(ValidNameBank, ValidNumberAccount, ValidDigitAccount);
        inactiveAccount.Deactivate();

        _repositoryMock
            .Setup(r => r.GetByAccountAsync(ValidNumberAccount, ValidDigitAccount, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveAccount);

        var command = new CreateBankAccountCommand(ValidNameBank, ValidNumberAccount, ValidDigitAccount);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.Equal(BankAccountMessages.Errors.AlreadyExistsInactive, result.Error);
    }

    // --- Criação com sucesso (unidade) ---

    [Fact]
    public async Task Handle_WhenAccountDoesNotExist_ShouldCreateAndReturnSuccess()
    {
        _repositoryMock
            .Setup(r => r.GetByAccountAsync(ValidNumberAccount, ValidDigitAccount, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BankAccountEntity?)null);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<BankAccountEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new CreateBankAccountCommand(ValidNameBank, ValidNumberAccount, ValidDigitAccount);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(ValidNameBank, result.Value.NameBank);
        Assert.Equal(ValidNumberAccount, result.Value.NumberAccount);
        Assert.Equal(ValidDigitAccount, result.Value.DigitAccount);

        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<BankAccountEntity>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
