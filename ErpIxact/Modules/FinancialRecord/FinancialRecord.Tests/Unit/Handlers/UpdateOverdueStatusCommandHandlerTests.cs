using FinancialRecord.Application.Commands.UpdateOverdueStatus;
using FinancialRecord.Domain.Repositories;
using Shared.Kernel;

namespace FinancialRecord.Tests.Unit.Handlers;

public class UpdateOverdueStatusCommandHandlerTests
{
    private readonly Mock<IFinancialRecordRepository> _repositoryMock;
    private readonly UpdateOverdueStatusCommandHandler _handler;

    public UpdateOverdueStatusCommandHandlerTests()
    {
        _repositoryMock = new Mock<IFinancialRecordRepository>();
        _handler = new UpdateOverdueStatusCommandHandler(_repositoryMock.Object);
    }

    // -------------------------------------------------------------------------
    // Atualização de status para Vencido
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Handle_ShouldCallUpdateOverdueStatusAsyncOnce()
    {
        _repositoryMock
            .Setup(r => r.UpdateOverdueStatusAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateOverdueStatusCommand();

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(
            r => r.UpdateOverdueStatusAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessResult()
    {
        _repositoryMock
            .Setup(r => r.UpdateOverdueStatusAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateOverdueStatusCommand();

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }
}
