using MediatR;
using Patners.Application.DTOs;
using Patners.Domain.Messages;
using Patners.Domain.Repositories;
using Shared.Kernel;
namespace Patners.Application.Queries.GetPartnerByName;

public class GetPartnerByNameQueryHandler:IRequestHandler<GetPartnerByNameQuery,Result<List<PartnerDto>>>
{
    private readonly IPartnersRepository _repository;
    
    public GetPartnerByNameQueryHandler(IPartnersRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<PartnerDto>>> Handle(GetPartnerByNameQuery request, CancellationToken cancellationToken)
    {
        var partners = await _repository.GetByNameAsync(request.Name, cancellationToken);

        if (partners.Count == 0)
        {
            return Result.NotFound<List<PartnerDto>>(PartnersMessages.Errors.NotFound);
        }
        
        var dtos = partners
            .Select(p => new PartnerDto(p.Id, p.DocNumber.Formatted, p.Name, p.Active))
            .ToList();

        return Result.Success(dtos);
    }
}
