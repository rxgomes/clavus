using FinancialRecord.Application.DTOs;
using MediatR;
using Shared.Kernel;

namespace FinancialRecord.Application.Queries.GetById;

public record GetFinancialRecordByIdQuery(Guid Id) : IRequest<Result<FinancialRecordDto>>;
