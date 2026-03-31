using BankAccount.Domain.Exceptions;
using BankAccount.Domain.Messages;
using Shared.Kernel.Entities;

namespace BankAccount.Domain.Entities;

public class BankAccount : BaseEntity
{
    public string NameBank { get; private set; } = null!;
    public string NumberAccount { get; private set; } = null!;
    public string DigitAccount { get; private set; } = null!;

    public BankAccount(string nameBank, string numberAccount, string digitAccount)
    {
        if (string.IsNullOrWhiteSpace(nameBank))
        {
            throw new DomainException(BankAccountMessages.Errors.NameBankRequired);
        }

        if (nameBank.Length > 25)
        {
            throw new DomainException(BankAccountMessages.Errors.NameBankTooLong);
        }

        if (string.IsNullOrWhiteSpace(numberAccount))
        {
            throw new DomainException(BankAccountMessages.Errors.NumberAccountRequired);
        }

        if (numberAccount.Length > 15)
        {
            throw new DomainException(BankAccountMessages.Errors.NumberAccountTooLong);
        }

        if (string.IsNullOrWhiteSpace(digitAccount))
        {
            throw new DomainException(BankAccountMessages.Errors.DigitAccountRequired);
        }

        if (digitAccount.Length > 5)
        {
            throw new DomainException(BankAccountMessages.Errors.DigitAccountTooLong);
        }

        NameBank = nameBank;
        NumberAccount = numberAccount;
        DigitAccount = digitAccount;
    }

    public void Deactivate()
    {
        Active = false;
        SetUpdatedAt();
    }

    protected BankAccount() { }
}
