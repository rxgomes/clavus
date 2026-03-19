using MediatR;
using Patners.Application.DTOs;
using Patners.Domain.Messages;
using Patners.Domain.Repositories;
using Shared.Kernel;
using Shared.Kernel.ValueObjects;

namespace Patners.Application.Commands.CreatePartner;

public class CreatePartnerCommandHandler : IRequestHandler<CreatePartnerCommand, Result<PartnerDto>>
{
    private readonly IPartnersRepository _repository;

    public CreatePartnerCommandHandler(IPartnersRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PartnerDto>> Handle(CreatePartnerCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByDocNumberAsync(request.DocNumber, cancellationToken);
        if (existing is not null)
        {
            return Result.Conflict<PartnerDto>(PartnersMessages.Errors.AlreadyExists);
        }

        var docNumber = new DocNumber(request.DocNumber);
        var partner = new Patners.Domain.Entities.Partners(docNumber, request.Name);

        await _repository.AddAsync(partner, cancellationToken);

        var dto = new PartnerDto(partner.Id, partner.DocNumber.Formatted, partner.Name, partner.Active);

        return Result.Success(dto);
    }
}
