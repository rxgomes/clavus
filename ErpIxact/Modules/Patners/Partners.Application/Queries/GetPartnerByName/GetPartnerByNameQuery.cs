using MediatR;
using Patners.Application.DTOs;
using Shared.Kernel;

namespace Patners.Application.Queries.GetPartnerByName;

public record GetPartnerByNameQuery(string Name) : IRequest<Result<List<PartnerDto>>>;
