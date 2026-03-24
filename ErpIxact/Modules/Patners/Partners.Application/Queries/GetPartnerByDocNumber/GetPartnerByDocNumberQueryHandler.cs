using MediatR;
using Patners.Application.DTOs;
using Patners.Domain.Messages;
using Patners.Domain.Repositories;
using Shared.Kernel;

namespace Patners.Application.Queries.GetPartnerByDocNumber;

public class GetPartnerByDocNumberQueryHandler : IRequestHandler<GetPartnerByDocNumberQuery, Result<PartnerDto>>
{
    private readonly IPartnersRepository _repository;

    public GetPartnerByDocNumberQueryHandler(IPartnersRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PartnerDto>> Handle(GetPartnerByDocNumberQuery request, CancellationToken cancellationToken)
    {
        var partner = await _repository.GetByDocNumberAsync(request.DocNumber, cancellationToken);

        if (partner is null)
        {
            return Result.NotFound<PartnerDto>(PartnersMessages.Errors.NotFound);
        }

        var dto = new PartnerDto(partner.Id, partner.DocNumber.Formatted, partner.Name, partner.Active);

        return Result.Success(dto);
    }
}