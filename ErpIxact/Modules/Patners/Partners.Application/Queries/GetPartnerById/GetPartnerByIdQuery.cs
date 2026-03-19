using MediatR;
using Patners.Application.DTOs;
using Shared.Kernel;

namespace Patners.Application.Queries.GetPartnerById;

public record GetPartnerByIdQuery(Guid Id) : IRequest<Result<PartnerDto>>;
