using CreditCard.Application.DTOs;
using MediatR;
using Shared.Kernel;

namespace CreditCard.Application.Commands.CreateCreditCard;

public record CreateCreditCardCommand(string Name, string Flag, int CloseDay, int DueDay)
    : IRequest<Result<CreditCardDto>>;
