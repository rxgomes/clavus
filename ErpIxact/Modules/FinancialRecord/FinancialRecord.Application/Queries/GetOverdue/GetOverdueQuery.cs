using FinancialRecord.Application.DTOs;
using MediatR;
using Shared.Kernel;

namespace FinancialRecord.Application.Queries.GetOverdue;

public record GetOverdueQuery(int Page = 1, int PageSize = 50)
    : IRequest<Result<PagedResultDto<FinancialRecordDto>>>;
