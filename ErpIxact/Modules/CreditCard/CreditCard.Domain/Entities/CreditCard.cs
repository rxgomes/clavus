using CreditCard.Domain.Exceptions;
using CreditCard.Domain.Messages;
using Shared.Kernel.Entities;

namespace CreditCard.Domain.Entities;

public class CreditCard : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Flag { get; private set; } = null!;
    public int CloseDay { get; private set; }
    public int DueDay { get; private set; }

    public CreditCard(string name, string flag, int closeDay, int dueDay)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(CreditCardMessages.Errors.NameRequired);
        }

        if (name.Length > 15)
        {
            throw new DomainException(CreditCardMessages.Errors.NameTooLong);
        }

        if (string.IsNullOrWhiteSpace(flag))
        {
            throw new DomainException(CreditCardMessages.Errors.FlagRequired);
        }

        if (flag.Length > 15)
        {
            throw new DomainException(CreditCardMessages.Errors.FlagTooLong);
        }

        if (closeDay is < 1 or > 31)
        {
            throw new DomainException(CreditCardMessages.Errors.CloseDayInvalid);
        }

        if (dueDay is < 1 or > 31)
        {
            throw new DomainException(CreditCardMessages.Errors.DueDayInvalid);
        }

        Name = name;
        Flag = flag;
        CloseDay = closeDay;
        DueDay = dueDay;
    }

    public void Deactivate()
    {
        Active = false;
        SetUpdatedAt();
    }

    protected CreditCard() { }
}
