using FinancialRecord.Application.DTOs;
using FinancialRecord.Domain.Messages;
using FinancialRecord.Domain.Repositories;
using MediatR;
using Shared.Kernel;

namespace FinancialRecord.Application.Queries.GetById;

public class GetFinancialRecordByIdQueryHandler
    : IRequestHandler<GetFinancialRecordByIdQuery, Result<FinancialRecordDto>>
{
    private readonly IFinancialRecordRepository _repository;

    public GetFinancialRecordByIdQueryHandler(IFinancialRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<FinancialRecordDto>> Handle(
        GetFinancialRecordByIdQuery request,
        CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (record is null)
        {
            return Result.NotFound<FinancialRecordDto>(FinancialRecordMessages.Errors.NotFound);
        }

        var dto = new FinancialRecordDto(
            record.Id,
            record.Description,
            record.Value,
            record.DueDate,
            record.TotalInstallment,
            record.Installment,
            record.Status,
            record.Active,
            record.DigitableLine,
            record.CardPurchaseId);

        return Result.Success(dto);
    }
}
