using FinancialRecord.Application.Commands.CreateFinancialRecord;
using FinancialRecord.Application.DTOs;
using FinancialRecord.Domain.Enums;
using FinancialRecord.Domain.Repositories;
using Shared.Kernel;

namespace FinancialRecord.Tests.Unit.Handlers;

public class CreateFinancialRecordCommandHandlerTests
{
    private readonly Mock<IFinancialRecordRepository> _repositoryMock;
    private readonly CreateFinancialRecordCommandHandler _handler;

    private static readonly DateOnly ValidDueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));

    public CreateFinancialRecordCommandHandlerTests()
    {
        _repositoryMock = new Mock<IFinancialRecordRepository>();
        _handler = new CreateFinancialRecordCommandHandler(_repositoryMock.Object);
    }

    // -------------------------------------------------------------------------
    // Parcela única (TotalInstallment = 1) — AddAsync deve ser chamado uma vez
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Handle_WhenSingleInstallment_ShouldCallAddAsyncOnce()
    {
        var command = new CreateFinancialRecordCommand(
            Description: "Conta de água",
            Value: 80.00m,
            DueDate: ValidDueDate,
            TotalInstallment: 1,
            Status: FinancialRecordStatus.Pending);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<FinancialRecordEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<FinancialRecordEntity>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _repositoryMock.Verify(
            r => r.AddRangeAsync(It.IsAny<IEnumerable<FinancialRecordEntity>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSingleInstallment_ShouldReturnDtoWithInstallmentOne()
    {
        var command = new CreateFinancialRecordCommand(
            Description: "Conta de água",
            Value: 80.00m,
            DueDate: ValidDueDate,
            TotalInstallment: 1,
            Status: FinancialRecordStatus.Pending);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<FinancialRecordEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value);
        Assert.Equal(1, result.Value[0].Installment);
    }

    // -------------------------------------------------------------------------
    // Múltiplas parcelas (TotalInstallment > 1) — AddRangeAsync deve ser chamado
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Handle_WhenMultipleInstallments_ShouldCallAddRangeAsyncOnce()
    {
        var command = new CreateFinancialRecordCommand(
            Description: "Financiamento",
            Value: 1000.00m,
            DueDate: ValidDueDate,
            TotalInstallment: 3,
            Status: FinancialRecordStatus.Pending);

        _repositoryMock
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<FinancialRecordEntity>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(
            r => r.AddRangeAsync(It.IsAny<IEnumerable<FinancialRecordEntity>>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<FinancialRecordEntity>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenMultipleInstallments_ShouldReturnDtoListWithCorrectCount()
    {
        var command = new CreateFinancialRecordCommand(
            Description: "Financiamento",
            Value: 1000.00m,
            DueDate: ValidDueDate,
            TotalInstallment: 3,
            Status: FinancialRecordStatus.Pending);

        _repositoryMock
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<FinancialRecordEntity>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.Count);
    }

    // -------------------------------------------------------------------------
    // Validações — deve retornar erro de validação e não chamar repositório
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Handle_WhenDescriptionIsEmpty_ShouldReturnValidationError()
    {
        var command = new CreateFinancialRecordCommand(
            Description: "",
            Value: 100.00m,
            DueDate: ValidDueDate,
            TotalInstallment: 1,
            Status: FinancialRecordStatus.Pending);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<FinancialRecordEntity>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenValueIsZero_ShouldReturnValidationError()
    {
        var command = new CreateFinancialRecordCommand(
            Description: "Conta válida",
            Value: 0m,
            DueDate: ValidDueDate,
            TotalInstallment: 1,
            Status: FinancialRecordStatus.Pending);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task Handle_WhenDueDateIsInThePast_ShouldReturnValidationError()
    {
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

        var command = new CreateFinancialRecordCommand(
            Description: "Conta válida",
            Value: 100.00m,
            DueDate: pastDate,
            TotalInstallment: 1,
            Status: FinancialRecordStatus.Pending);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task Handle_WhenTotalInstallmentIsZero_ShouldReturnValidationError()
    {
        var command = new CreateFinancialRecordCommand(
            Description: "Conta válida",
            Value: 100.00m,
            DueDate: ValidDueDate,
            TotalInstallment: 0,
            Status: FinancialRecordStatus.Pending);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task Handle_WhenPaidValueIsLessThanValue_ShouldReturnValidationError()
    {
        var command = new CreateFinancialRecordCommand(
            Description: "Conta válida",
            Value: 200.00m,
            DueDate: ValidDueDate,
            TotalInstallment: 1,
            Status: FinancialRecordStatus.Pending,
            PaidValue: 100.00m);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
    }
}
