using FinancialRecord.Application.DTOs;
using MediatR;
using Shared.Kernel;

namespace FinancialRecord.Application.Queries.GetByMonthYear;

public record GetByMonthYearQuery(int Month, int Year, int Page = 1, int PageSize = 50)
    : IRequest<Result<PagedResultDto<FinancialRecordDto>>>;
