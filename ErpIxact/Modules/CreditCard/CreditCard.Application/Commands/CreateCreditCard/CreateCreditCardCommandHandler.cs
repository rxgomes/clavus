using CreditCard.Application.DTOs;
using CreditCard.Domain.Messages;
using CreditCard.Domain.Repositories;
using MediatR;
using Shared.Kernel;
using CreditCardEntity = CreditCard.Domain.Entities.CreditCard;

namespace CreditCard.Application.Commands.CreateCreditCard;

public class CreateCreditCardCommandHandler : IRequestHandler<CreateCreditCardCommand, Result<CreditCardDto>>
{
    private readonly ICreditCardRepository _repository;

    public CreateCreditCardCommandHandler(ICreditCardRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CreditCardDto>> Handle(CreateCreditCardCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByNameAndFlagAsync(request.Name, request.Flag, cancellationToken);

        if (existing is not null)
        {
            if (existing.Active)
            {
                return Result.Conflict<CreditCardDto>(CreditCardMessages.Errors.AlreadyExistsActive);
            }

            return Result.Conflict<CreditCardDto>(CreditCardMessages.Errors.AlreadyExistsInactive);
        }

        var card = new CreditCardEntity(request.Name, request.Flag, request.CloseDay, request.DueDay);
        await _repository.AddAsync(card, cancellationToken);

        var dto = new CreditCardDto(card.Id, card.Name, card.Flag, card.CloseDay, card.DueDay, card.Active);
        return Result.Success(dto);
    }
}
