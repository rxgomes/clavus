using MediatR;
using Shared.Kernel;

namespace FinancialRecord.Application.Commands.DeactivateFinancialRecord;

public record DeactivateFinancialRecordCommand(Guid Id) : IRequest<Result<string>>;
