using MediatR;
using Patners.Application.DTOs;
using Patners.Domain.Messages;
using Patners.Domain.Repositories;
using Shared.Kernel;
using Shared.Kernel.ValueObjects;

namespace Patners.Application.Commands.UpdatePartner;

public class UpdatePartnerCommandHandler : IRequestHandler<UpdatePartnerCommand, Result<PartnerDto>>
{
    private readonly IPartnersRepository _repository;

    public UpdatePartnerCommandHandler(IPartnersRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<Result<PartnerDto>> Handle(UpdatePartnerCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (existing is null)
        {
            return Result.NotFound<PartnerDto>(PartnersMessages.Errors.NotFound);
        }

        var docWithSameNumber = await _repository.GetByDocNumberAsync(request.DocNum, cancellationToken);
        if (docWithSameNumber is not null && docWithSameNumber.Id != existing.Id)
        {
            return Result.Conflict<PartnerDto>(PartnersMessages.Errors.AlreadyExists);
        }

        var docNum = new DocNumber(request.DocNum);
        existing.Update(docNum, request.Name);

        await _repository.UpdateAsync(existing, cancellationToken);

        var dto = new PartnerDto(existing.Id, existing.DocNumber.Formatted, existing.Name, existing.Active);

        return Result.Success(dto);
    }
}
