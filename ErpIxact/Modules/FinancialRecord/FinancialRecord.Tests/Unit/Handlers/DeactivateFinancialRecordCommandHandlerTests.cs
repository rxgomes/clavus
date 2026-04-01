using FinancialRecord.Application.Commands.DeactivateFinancialRecord;
using FinancialRecord.Domain.Enums;
using FinancialRecord.Domain.Messages;
using FinancialRecord.Domain.Repositories;
using Shared.Kernel;

namespace FinancialRecord.Tests.Unit.Handlers;

public class DeactivateFinancialRecordCommandHandlerTests
{
    private readonly Mock<IFinancialRecordRepository> _repositoryMock;
    private readonly DeactivateFinancialRecordCommandHandler _handler;

    private static readonly DateOnly ValidDueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));

    public DeactivateFinancialRecordCommandHandlerTests()
    {
        _repositoryMock = new Mock<IFinancialRecordRepository>();
        _handler = new DeactivateFinancialRecordCommandHandler(_repositoryMock.Object);
    }

    // -------------------------------------------------------------------------
    // Registro não encontrado
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Handle_WhenRecordNotFound_ShouldReturnNotFound()
    {
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FinancialRecordEntity?)null);

        var command = new DeactivateFinancialRecordCommand(id);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        Assert.Equal(FinancialRecordMessages.Errors.NotFound, result.Error);
        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<FinancialRecordEntity>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // -------------------------------------------------------------------------
    // Exclusão lógica com sucesso
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Handle_WhenRecordExists_ShouldDeactivateAndCallUpdateAsync()
    {
        var record = new FinancialRecordEntity(
            "Conta de luz", 150.00m, ValidDueDate, 1, FinancialRecordStatus.Pending);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(record.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<FinancialRecordEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new DeactivateFinancialRecordCommand(record.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(record.Active);
        _repositoryMock.Verify(
            r => r.UpdateAsync(record, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRecordExists_ShouldReturnSuccessMessage()
    {
        var record = new FinancialRecordEntity(
            "Conta de luz", 150.00m, ValidDueDate, 1, FinancialRecordStatus.Pending);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(record.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<FinancialRecordEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new DeactivateFinancialRecordCommand(record.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(FinancialRecordMessages.Success.Deactivated, result.Value);
    }
}
