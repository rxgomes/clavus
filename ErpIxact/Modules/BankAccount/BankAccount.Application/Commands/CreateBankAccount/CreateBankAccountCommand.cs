using BankAccount.Application.DTOs;
using MediatR;
using Shared.Kernel;

namespace BankAccount.Application.Commands.CreateBankAccount;

public record CreateBankAccountCommand(string NameBank, string NumberAccount, string DigitAccount)
    : IRequest<Result<BankAccountDto>>;
