using FinancialRecord.Application.DTOs;
using FinancialRecord.Domain.Enums;
using MediatR;
using Shared.Kernel;

namespace FinancialRecord.Application.Queries.GetByStatus;

public record GetByStatusQuery(FinancialRecordStatus Status, int Page = 1, int PageSize = 50)
    : IRequest<Result<PagedResultDto<FinancialRecordDto>>>;
