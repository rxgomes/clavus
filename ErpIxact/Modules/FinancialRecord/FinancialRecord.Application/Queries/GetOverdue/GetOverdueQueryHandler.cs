using FinancialRecord.Application.DTOs;
using FinancialRecord.Domain.Repositories;
using MediatR;
using Shared.Kernel;

namespace FinancialRecord.Application.Queries.GetOverdue;

public class GetOverdueQueryHandler
    : IRequestHandler<GetOverdueQuery, Result<PagedResultDto<FinancialRecordDto>>>
{
    private readonly IFinancialRecordRepository _repository;

    public GetOverdueQueryHandler(IFinancialRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResultDto<FinancialRecordDto>>> Handle(
        GetOverdueQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetOverdueAsync(
            request.Page, request.PageSize, cancellationToken);

        var dtos = items.Select(r => new FinancialRecordDto(
            r.Id, r.Description, r.Value, r.DueDate,
            r.TotalInstallment, r.Installment, r.Status, r.Active)).ToList();

        return Result.Success(new PagedResultDto<FinancialRecordDto>(dtos, totalCount, request.Page, request.PageSize));
    }
}
