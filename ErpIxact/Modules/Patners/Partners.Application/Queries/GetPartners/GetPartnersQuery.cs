using MediatR;
using Patners.Application.DTOs;
using Shared.Kernel;

namespace Patners.Application.Queries.GetPartners;

public record GetPartnersQuery() : IRequest<Result<List<PartnerDto>>>;