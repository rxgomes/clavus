using FinancialRecord.Domain.Messages;
using FinancialRecord.Domain.Repositories;
using MediatR;
using Shared.Kernel;

namespace FinancialRecord.Application.Commands.UpdateOverdueStatus;

public class UpdateOverdueStatusCommandHandler : IRequestHandler<UpdateOverdueStatusCommand, Result<string>>
{
    private readonly IFinancialRecordRepository _repository;

    public UpdateOverdueStatusCommandHandler(IFinancialRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<string>> Handle(UpdateOverdueStatusCommand request, CancellationToken cancellationToken)
    {
        await _repository.UpdateOverdueStatusAsync(cancellationToken);
        return Result.Success(FinancialRecordMessages.Success.OverdueUpdated);
    }
}
