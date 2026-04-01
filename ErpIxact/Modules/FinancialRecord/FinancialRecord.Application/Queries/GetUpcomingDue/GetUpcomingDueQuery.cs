using FinancialRecord.Application.DTOs;
using MediatR;
using Shared.Kernel;

namespace FinancialRecord.Application.Queries.GetUpcomingDue;

public record GetUpcomingDueQuery(int Days, int Page = 1, int PageSize = 50)
    : IRequest<Result<PagedResultDto<FinancialRecordDto>>>;
