namespace CreditCard.Application.DTOs;

public record CreditCardDto(Guid Id, string Name, string Flag, int CloseDay, int DueDay, bool Active);
