using FinancialRecord.Domain.Messages;
using FinancialRecord.Domain.Repositories;
using MediatR;
using Shared.Kernel;

namespace FinancialRecord.Application.Commands.DeactivateFinancialRecord;

public class DeactivateFinancialRecordCommandHandler : IRequestHandler<DeactivateFinancialRecordCommand, Result<string>>
{
    private readonly IFinancialRecordRepository _repository;

    public DeactivateFinancialRecordCommandHandler(IFinancialRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<string>> Handle(DeactivateFinancialRecordCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (record is null)
        {
            return Result.NotFound<string>(FinancialRecordMessages.Errors.NotFound);
        }

        record.Deactivate();
        await _repository.UpdateAsync(record, cancellationToken);

        return Result.Success(FinancialRecordMessages.Success.Deactivated);
    }
}
