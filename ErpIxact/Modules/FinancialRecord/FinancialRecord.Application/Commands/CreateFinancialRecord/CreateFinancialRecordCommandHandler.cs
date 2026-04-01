using FinancialRecord.Application.DTOs;
using FinancialRecord.Domain.Exceptions;
using FinancialRecord.Domain.Repositories;
using MediatR;
using Shared.Kernel;
using FinancialRecordEntity = FinancialRecord.Domain.Entities.FinancialRecord;

namespace FinancialRecord.Application.Commands.CreateFinancialRecord;

public class CreateFinancialRecordCommandHandler : IRequestHandler<CreateFinancialRecordCommand, Result<List<FinancialRecordDto>>>
{
    private readonly IFinancialRecordRepository _repository;

    public CreateFinancialRecordCommandHandler(IFinancialRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<FinancialRecordDto>>> Handle(CreateFinancialRecordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.TotalInstallment == 1)
            {
                var record = new FinancialRecordEntity(
                    request.Description,
                    request.Value,
                    request.DueDate,
                    request.TotalInstallment,
                    request.Status,
                    paidValue: request.PaidValue,
                    paymentDate: request.PaymentDate,
                    digitableLine: request.DigitableLine);

                await _repository.AddAsync(record, cancellationToken);

                return Result.Success(new List<FinancialRecordDto> { ToDto(record) });
            }
            else
            {
                var records = FinancialRecordEntity.CreateInstallments(
                    request.Description,
                    request.Value,
                    request.DueDate,
                    request.TotalInstallment,
                    request.Status,
                    request.DigitableLine);

                await _repository.AddRangeAsync(records, cancellationToken);

                return Result.Success(records.Select(ToDto).ToList());
            }
        }
        catch (DomainException ex)
        {
            return Result.Failure<List<FinancialRecordDto>>(ex.Message);
        }
    }

    private static FinancialRecordDto ToDto(FinancialRecordEntity record)
        => new(record.Id, record.Description, record.Value, record.DueDate, record.TotalInstallment, record.Installment, record.Status, record.Active);
}
