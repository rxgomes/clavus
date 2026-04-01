using FinancialRecord.Application.DTOs;
using MediatR;
using Shared.Kernel;

namespace FinancialRecord.Application.Queries.GetCurrentMonth;

public record GetCurrentMonthQuery(int Page = 1, int PageSize = 50)
    : IRequest<Result<PagedResultDto<FinancialRecordDto>>>;
