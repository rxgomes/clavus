using CreditCard.Application.Commands.CreateCreditCard;
using CreditCard.Domain.Messages;
using CreditCard.Domain.Repositories;
using Moq;
using Shared.Kernel;
using Xunit;
using CreditCardEntity = CreditCard.Domain.Entities.CreditCard;

namespace CreditCard.Tests.Unit.Handlers;

public class CreateCreditCardCommandHandlerTests
{
    private readonly Mock<ICreditCardRepository> _repositoryMock;
    private readonly CreateCreditCardCommandHandler _handler;

    private const string ValidName = "Meu Cartão";
    private const string ValidFlag = "Visa";
    private const int ValidCloseDay = 10;
    private const int ValidDueDay = 20;

    public CreateCreditCardCommandHandlerTests()
    {
        _repositoryMock = new Mock<ICreditCardRepository>();
        _handler = new CreateCreditCardCommandHandler(_repositoryMock.Object);
    }

    // --- Verificação de duplicidade ---

    [Fact]
    public async Task Handle_WhenCardAlreadyExistsAndIsActive_ShouldReturnConflictWithActiveMessage()
    {
        var existingCard = new CreditCardEntity(ValidName, ValidFlag, ValidCloseDay, ValidDueDay);

        _repositoryMock
            .Setup(r => r.GetByNameAndFlagAsync(ValidName, ValidFlag, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCard);

        var command = new CreateCreditCardCommand(ValidName, ValidFlag, ValidCloseDay, ValidDueDay);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.Equal(CreditCardMessages.Errors.AlreadyExistsActive, result.Error);
    }

    [Fact]
    public async Task Handle_WhenCardAlreadyExistsAndIsInactive_ShouldReturnConflictWithInactiveMessage()
    {
        var inactiveCard = new CreditCardEntity(ValidName, ValidFlag, ValidCloseDay, ValidDueDay);
        inactiveCard.Deactivate();

        _repositoryMock
            .Setup(r => r.GetByNameAndFlagAsync(ValidName, ValidFlag, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveCard);

        var command = new CreateCreditCardCommand(ValidName, ValidFlag, ValidCloseDay, ValidDueDay);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.Equal(CreditCardMessages.Errors.AlreadyExistsInactive, result.Error);
    }

    // --- Criação com sucesso ---

    [Fact]
    public async Task Handle_WhenCardDoesNotExist_ShouldCreateAndReturnSuccess()
    {
        _repositoryMock
            .Setup(r => r.GetByNameAndFlagAsync(ValidName, ValidFlag, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreditCardEntity?)null);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<CreditCardEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new CreateCreditCardCommand(ValidName, ValidFlag, ValidCloseDay, ValidDueDay);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(ValidName, result.Value.Name);
        Assert.Equal(ValidFlag, result.Value.Flag);
        Assert.Equal(ValidCloseDay, result.Value.CloseDay);
        Assert.Equal(ValidDueDay, result.Value.DueDay);

        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<CreditCardEntity>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
