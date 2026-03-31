namespace BankAccount.Application.DTOs;

public record BankAccountDto(Guid Id, string NameBank, string NumberAccount, string DigitAccount, bool Active);
