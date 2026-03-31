using BankAccount.Application.DTOs;
using BankAccount.Domain.Messages;
using BankAccount.Domain.Repositories;
using MediatR;
using Shared.Kernel;
using BankAccountEntity = BankAccount.Domain.Entities.BankAccount;

namespace BankAccount.Application.Commands.CreateBankAccount;

public class CreateBankAccountCommandHandler : IRequestHandler<CreateBankAccountCommand, Result<BankAccountDto>>
{
    private readonly IBankAccountRepository _repository;

    public CreateBankAccountCommandHandler(IBankAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<BankAccountDto>> Handle(CreateBankAccountCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByAccountAsync(request.NumberAccount, request.DigitAccount, cancellationToken);

        if (existing is not null)
        {
            if (existing.Active)
            {
                return Result.Conflict<BankAccountDto>(BankAccountMessages.Errors.AlreadyExistsActive);
            }

            return Result.Conflict<BankAccountDto>(BankAccountMessages.Errors.AlreadyExistsInactive);
        }

        var bankAccount = new BankAccountEntity(request.NameBank, request.NumberAccount, request.DigitAccount);

        await _repository.AddAsync(bankAccount, cancellationToken);

        var dto = new BankAccountDto(bankAccount.Id, bankAccount.NameBank, bankAccount.NumberAccount, bankAccount.DigitAccount, bankAccount.Active);

        return Result.Success(dto);
    }
}
