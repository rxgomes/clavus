using MediatR;
using Patners.Application.DTOs;
using Shared.Kernel;

namespace Patners.Application.Queries.GetPartnerByDocNumber;

public record GetPartnerByDocNumberQuery(string DocNumber) : IRequest<Result<PartnerDto>>;