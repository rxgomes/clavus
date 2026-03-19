using MediatR;
using Patners.Application.DTOs;
using Patners.Domain.Repositories;
using Shared.Kernel;

namespace Patners.Application.Queries.GetPartners;

public class GetPartnersQueryHandler : IRequestHandler<GetPartnersQuery, Result<List<PartnerDto>>>
{
    private readonly IPartnersRepository _repository;

    public GetPartnersQueryHandler(IPartnersRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<PartnerDto>>> Handle(GetPartnersQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}