using FinancialRecord.Domain.Enums;
using FinancialRecord.Domain.Exceptions;
using FinancialRecord.Domain.Messages;
using Shared.Kernel.Entities;

namespace FinancialRecord.Domain.Entities;

public class FinancialRecord : BaseEntity
{
    public string Description { get; private set; } = null!;
    public decimal Value { get; private set; }
    public DateOnly DueDate { get; private set; }
    public int TotalInstallment { get; private set; }
    public FinancialRecordStatus Status { get; private set; }
    public int Installment { get; private set; }
    public DateOnly? PaymentDate { get; private set; }
    public decimal? PaidValue { get; private set; }
    public string? DigitableLine { get; private set; }

    public FinancialRecord(
        string description,
        decimal value,
        DateOnly dueDate,
        int totalInstallment,
        FinancialRecordStatus status,
        int installment = 1,
        DateOnly? paymentDate = null,
        decimal? paidValue = null,
        string? digitableLine = null)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException(FinancialRecordMessages.Errors.DescriptionRequired);
        }

        if (description.Length > 100)
        {
            throw new DomainException(FinancialRecordMessages.Errors.DescriptionTooLong);
        }

        if (value <= 0)
        {
            throw new DomainException(FinancialRecordMessages.Errors.ValueRequired);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (dueDate < today)
        {
            throw new DomainException(FinancialRecordMessages.Errors.DueDateInvalid);
        }

        if (totalInstallment <= 0)
        {
            throw new DomainException(FinancialRecordMessages.Errors.TotalInstallmentInvalid);
        }

        if (!Enum.IsDefined(typeof(FinancialRecordStatus), status))
        {
            throw new DomainException(FinancialRecordMessages.Errors.StatusInvalid);
        }

        if (paidValue.HasValue)
        {
            if (paidValue.Value <= 0)
            {
                throw new DomainException(FinancialRecordMessages.Errors.PaidValueInvalid);
            }

            if (paidValue.Value < value)
            {
                throw new DomainException(FinancialRecordMessages.Errors.PaidValuePartialNotAllowed);
            }
        }

        if (!string.IsNullOrEmpty(digitableLine))
        {
            if (digitableLine.Length > 50)
            {
                throw new DomainException(FinancialRecordMessages.Errors.DigitableLineTooLong);
            }

            if (!digitableLine.All(char.IsDigit))
            {
                throw new DomainException(FinancialRecordMessages.Errors.DigitableLineOnlyDigits);
            }
        }

        Description = description;
        Value = value;
        DueDate = dueDate;
        TotalInstallment = totalInstallment;
        Status = status;
        Installment = installment;
        DigitableLine = digitableLine;
        PaidValue = paidValue;
        PaymentDate = paidValue.HasValue
            ? (paymentDate ?? DateOnly.FromDateTime(DateTime.UtcNow))
            : paymentDate;
    }

    protected FinancialRecord() { }

    /// <summary>
    /// Cria N registros de parcelas a partir dos dados informados.
    /// Se TotalInstallment = 1, retorna um único registro com Installment = 1.
    /// Se TotalInstallment > 1, retorna N registros com DueDate incrementado mensalmente.
    /// </summary>
    public static IReadOnlyList<FinancialRecord> CreateInstallments(
        string description,
        decimal value,
        DateOnly dueDate,
        int totalInstallment,
        FinancialRecordStatus status,
        string? digitableLine = null)
    {
        if (totalInstallment <= 0)
        {
            throw new DomainException(FinancialRecordMessages.Errors.TotalInstallmentInvalid);
        }

        var records = new List<FinancialRecord>();

        for (var i = 0; i < totalInstallment; i++)
        {
            var installmentDueDate = dueDate.AddMonths(i);
            records.Add(new FinancialRecord(
                description, value, installmentDueDate,
                totalInstallment, status,
                installment: i + 1,
                digitableLine: digitableLine));
        }

        return records;
    }

    public void UpdatePaidValue(decimal paidValue, DateOnly? paymentDate = null)
    {
        if (paidValue <= 0)
        {
            throw new DomainException(FinancialRecordMessages.Errors.PaidValueInvalid);
        }

        if (paidValue < Value)
        {
            throw new DomainException(FinancialRecordMessages.Errors.PaidValuePartialNotAllowed);
        }

        PaidValue = paidValue;
        PaymentDate = paymentDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        SetUpdatedAt();
    }

    public void MarkAsOverdue()
    {
        Status = FinancialRecordStatus.Overdue;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        Active = false;
        SetUpdatedAt();
    }
}
