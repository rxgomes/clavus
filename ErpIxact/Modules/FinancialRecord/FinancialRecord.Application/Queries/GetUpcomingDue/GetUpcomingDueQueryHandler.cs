using FinancialRecord.Application.DTOs;
using FinancialRecord.Domain.Repositories;
using MediatR;
using Shared.Kernel;

namespace FinancialRecord.Application.Queries.GetUpcomingDue;

public class GetUpcomingDueQueryHandler
    : IRequestHandler<GetUpcomingDueQuery, Result<PagedResultDto<FinancialRecordDto>>>
{
    private readonly IFinancialRecordRepository _repository;

    public GetUpcomingDueQueryHandler(IFinancialRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResultDto<FinancialRecordDto>>> Handle(
        GetUpcomingDueQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetUpcomingDueAsync(
            request.Days, request.Page, request.PageSize, cancellationToken);

        var dtos = items.Select(r => new FinancialRecordDto(
            r.Id, r.Description, r.Value, r.DueDate,
            r.TotalInstallment, r.Installment, r.Status, r.Active, r.DigitableLine, r.CardPurchaseId)).ToList();

        return Result.Success(new PagedResultDto<FinancialRecordDto>(dtos, totalCount, request.Page, request.PageSize));
    }
}
