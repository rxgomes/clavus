using MediatR;
using Shared.Kernel;

namespace FinancialRecord.Application.Commands.UpdateOverdueStatus;

public record UpdateOverdueStatusCommand() : IRequest<Result<string>>;
