using FinancialRecord.Application.DTOs;
using FinancialRecord.Domain.Repositories;
using MediatR;
using Shared.Kernel;

namespace FinancialRecord.Application.Queries.GetByMonthYear;

public class GetByMonthYearQueryHandler
    : IRequestHandler<GetByMonthYearQuery, Result<PagedResultDto<FinancialRecordDto>>>
{
    private readonly IFinancialRecordRepository _repository;

    public GetByMonthYearQueryHandler(IFinancialRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResultDto<FinancialRecordDto>>> Handle(
        GetByMonthYearQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetByMonthYearAsync(
            request.Month, request.Year, request.Page, request.PageSize, cancellationToken);

        var dtos = items.Select(r => new FinancialRecordDto(
            r.Id, r.Description, r.Value, r.DueDate,
            r.TotalInstallment, r.Installment, r.Status, r.Active)).ToList();

        return Result.Success(new PagedResultDto<FinancialRecordDto>(dtos, totalCount, request.Page, request.PageSize));
    }
}
