using FinancialRecord.Domain.Enums;
using FinancialRecord.Domain.Exceptions;
using FinancialRecord.Domain.Messages;

namespace FinancialRecord.Tests.Unit.Entities;

public class FinancialRecordEntityTests
{
    // Dados válidos de base para reuso nos testes
    private static readonly string ValidDescription = "Conta de luz";
    private static readonly decimal ValidValue = 100.00m;
    private static readonly DateOnly ValidDueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));
    private static readonly int ValidTotalInstallment = 1;
    private static readonly FinancialRecordStatus ValidStatus = FinancialRecordStatus.Pending;

    // -------------------------------------------------------------------------
    // Description — obrigatório, máximo 100 caracteres
    // -------------------------------------------------------------------------

    [Fact]
    public void Constructor_WhenDescriptionIsNull_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new FinancialRecordEntity(null!, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus));

        Assert.Equal(FinancialRecordMessages.Errors.DescriptionRequired, exception.Message);
    }

    [Fact]
    public void Constructor_WhenDescriptionIsEmpty_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new FinancialRecordEntity(string.Empty, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus));

        Assert.Equal(FinancialRecordMessages.Errors.DescriptionRequired, exception.Message);
    }

    [Fact]
    public void Constructor_WhenDescriptionIsWhiteSpace_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new FinancialRecordEntity("   ", ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus));

        Assert.Equal(FinancialRecordMessages.Errors.DescriptionRequired, exception.Message);
    }

    [Fact]
    public void Constructor_WhenDescriptionExceeds100Characters_ShouldThrowDomainException()
    {
        var longDescription = new string('A', 101);

        var exception = Assert.Throws<DomainException>(() =>
            new FinancialRecordEntity(longDescription, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus));

        Assert.Equal(FinancialRecordMessages.Errors.DescriptionTooLong, exception.Message);
    }

    [Fact]
    public void Constructor_WhenDescriptionIsExactly100Characters_ShouldNotThrow()
    {
        var descriptionAt100 = new string('A', 100);

        var exception = Record.Exception(() =>
            new FinancialRecordEntity(descriptionAt100, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus));

        Assert.Null(exception);
    }

    // -------------------------------------------------------------------------
    // Value — obrigatório, deve ser > 0
    // -------------------------------------------------------------------------

    [Fact]
    public void Constructor_WhenValueIsZero_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new FinancialRecordEntity(ValidDescription, 0m, ValidDueDate, ValidTotalInstallment, ValidStatus));

        Assert.Equal(FinancialRecordMessages.Errors.ValueRequired, exception.Message);
    }

    [Fact]
    public void Constructor_WhenValueIsNegative_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new FinancialRecordEntity(ValidDescription, -1m, ValidDueDate, ValidTotalInstallment, ValidStatus));

        Assert.Equal(FinancialRecordMessages.Errors.ValueRequired, exception.Message);
    }

    // -------------------------------------------------------------------------
    // DueDate — obrigatório, não pode ser menor que a data atual
    // -------------------------------------------------------------------------

    [Fact]
    public void Constructor_WhenDueDateIsInThePast_ShouldThrowDomainException()
    {
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

        var exception = Assert.Throws<DomainException>(() =>
            new FinancialRecordEntity(ValidDescription, ValidValue, pastDate, ValidTotalInstallment, ValidStatus));

        Assert.Equal(FinancialRecordMessages.Errors.DueDateInvalid, exception.Message);
    }

    [Fact]
    public void Constructor_WhenDueDateIsToday_ShouldNotThrow()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var exception = Record.Exception(() =>
            new FinancialRecordEntity(ValidDescription, ValidValue, today, ValidTotalInstallment, ValidStatus));

        Assert.Null(exception);
    }

    [Fact]
    public void Constructor_WhenDueDateIsInTheFuture_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
            new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus));

        Assert.Null(exception);
    }

    // -------------------------------------------------------------------------
    // TotalInstallment — obrigatório, deve ser > 0
    // -------------------------------------------------------------------------

    [Fact]
    public void Constructor_WhenTotalInstallmentIsZero_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, 0, ValidStatus));

        Assert.Equal(FinancialRecordMessages.Errors.TotalInstallmentInvalid, exception.Message);
    }

    [Fact]
    public void Constructor_WhenTotalInstallmentIsNegative_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, -1, ValidStatus));

        Assert.Equal(FinancialRecordMessages.Errors.TotalInstallmentInvalid, exception.Message);
    }

    // -------------------------------------------------------------------------
    // PaidValue — opcional; quando informado, deve ser >= Value (sem pagamento parcial)
    // -------------------------------------------------------------------------

    [Fact]
    public void Constructor_WhenPaidValueIsZero_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus,
                paidValue: 0m));

        Assert.Equal(FinancialRecordMessages.Errors.PaidValueInvalid, exception.Message);
    }

    [Fact]
    public void Constructor_WhenPaidValueIsNegative_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus,
                paidValue: -10m));

        Assert.Equal(FinancialRecordMessages.Errors.PaidValueInvalid, exception.Message);
    }

    [Fact]
    public void Constructor_WhenPaidValueIsLessThanValue_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus,
                paidValue: ValidValue - 0.01m));

        Assert.Equal(FinancialRecordMessages.Errors.PaidValuePartialNotAllowed, exception.Message);
    }

    [Fact]
    public void Constructor_WhenPaidValueEqualsValue_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
            new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus,
                paidValue: ValidValue));

        Assert.Null(exception);
    }

    [Fact]
    public void Constructor_WhenPaidValueIsGreaterThanValue_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
            new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus,
                paidValue: ValidValue + 1m));

        Assert.Null(exception);
    }

    // -------------------------------------------------------------------------
    // PaymentDate — quando PaidValue é informado e PaymentDate é nulo, deve ser a data atual
    // -------------------------------------------------------------------------

    [Fact]
    public void Constructor_WhenPaidValueIsProvidedAndPaymentDateIsNull_ShouldDefaultPaymentDateToToday()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var record = new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus,
            paidValue: ValidValue,
            paymentDate: null);

        Assert.Equal(today, record.PaymentDate);
    }

    [Fact]
    public void Constructor_WhenPaidValueIsProvidedWithPaymentDate_ShouldUseProvidedPaymentDate()
    {
        var specificDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2));

        var record = new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus,
            paidValue: ValidValue,
            paymentDate: specificDate);

        Assert.Equal(specificDate, record.PaymentDate);
    }

    [Fact]
    public void Constructor_WhenPaidValueIsNotProvided_ShouldLeavePaymentDateNull()
    {
        var record = new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus);

        Assert.Null(record.PaymentDate);
    }

    // -------------------------------------------------------------------------
    // DigitableLine — opcional, máximo 50 caracteres, apenas dígitos
    // -------------------------------------------------------------------------

    [Fact]
    public void Constructor_WhenDigitableLineExceeds50Characters_ShouldThrowDomainException()
    {
        var longLine = new string('1', 51);

        var exception = Assert.Throws<DomainException>(() =>
            new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus,
                digitableLine: longLine));

        Assert.Equal(FinancialRecordMessages.Errors.DigitableLineTooLong, exception.Message);
    }

    [Fact]
    public void Constructor_WhenDigitableLineIsExactly50Characters_ShouldNotThrow()
    {
        var lineAt50 = new string('1', 50);

        var exception = Record.Exception(() =>
            new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus,
                digitableLine: lineAt50));

        Assert.Null(exception);
    }

    [Fact]
    public void Constructor_WhenDigitableLineContainsNonDigitCharacters_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus,
                digitableLine: "1234ABC56"));

        Assert.Equal(FinancialRecordMessages.Errors.DigitableLineOnlyDigits, exception.Message);
    }

    [Fact]
    public void Constructor_WhenDigitableLineContainsDots_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus,
                digitableLine: "1234.5678"));

        Assert.Equal(FinancialRecordMessages.Errors.DigitableLineOnlyDigits, exception.Message);
    }

    [Fact]
    public void Constructor_WhenDigitableLineIsNull_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
            new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus,
                digitableLine: null));

        Assert.Null(exception);
    }

    [Fact]
    public void Constructor_WhenDigitableLineIsValidDigits_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
            new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus,
                digitableLine: "12345678901234567890"));

        Assert.Null(exception);
    }

    // -------------------------------------------------------------------------
    // Status — deve ser Pending, Overdue ou Paid
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(FinancialRecordStatus.Pending)]
    [InlineData(FinancialRecordStatus.Overdue)]
    [InlineData(FinancialRecordStatus.Paid)]
    public void Constructor_WhenStatusIsValid_ShouldNotThrow(FinancialRecordStatus status)
    {
        var exception = Record.Exception(() =>
            new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, ValidTotalInstallment, status));

        Assert.Null(exception);
    }

    [Fact]
    public void Constructor_WhenStatusIsInvalidEnumValue_ShouldThrowDomainException()
    {
        var invalidStatus = (FinancialRecordStatus)99;

        var exception = Assert.Throws<DomainException>(() =>
            new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, ValidTotalInstallment, invalidStatus));

        Assert.Equal(FinancialRecordMessages.Errors.StatusInvalid, exception.Message);
    }

    // -------------------------------------------------------------------------
    // Installment — quando TotalInstallment = 1, Installment deve ser 1
    // -------------------------------------------------------------------------

    [Fact]
    public void Constructor_WhenTotalInstallmentIsOne_ShouldSetInstallmentToOne()
    {
        var record = new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, 1, ValidStatus);

        Assert.Equal(1, record.Installment);
        Assert.Equal(1, record.TotalInstallment);
    }

    // -------------------------------------------------------------------------
    // Criação com sucesso — todos os campos válidos
    // -------------------------------------------------------------------------

    [Fact]
    public void Constructor_WhenAllRequiredFieldsAreValid_ShouldCreateRecord()
    {
        var record = new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus);

        Assert.Equal(ValidDescription, record.Description);
        Assert.Equal(ValidValue, record.Value);
        Assert.Equal(ValidDueDate, record.DueDate);
        Assert.Equal(ValidTotalInstallment, record.TotalInstallment);
        Assert.Equal(ValidStatus, record.Status);
        Assert.Equal(1, record.Installment);
        Assert.Null(record.PaymentDate);
        Assert.Null(record.PaidValue);
        Assert.Null(record.DigitableLine);
        Assert.True(record.Active);
        Assert.NotEqual(Guid.Empty, record.Id);
    }

    [Fact]
    public void Constructor_WhenAllFieldsAreValid_ShouldCreateRecord()
    {
        var paymentDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var record = new FinancialRecordEntity(
            description: "Conta de energia elétrica",
            value: 250.00m,
            dueDate: ValidDueDate,
            totalInstallment: 1,
            status: FinancialRecordStatus.Paid,
            installment: 1,
            paymentDate: paymentDate,
            paidValue: 250.00m,
            digitableLine: "12345678901234567890");

        Assert.Equal("Conta de energia elétrica", record.Description);
        Assert.Equal(250.00m, record.Value);
        Assert.Equal(ValidDueDate, record.DueDate);
        Assert.Equal(1, record.TotalInstallment);
        Assert.Equal(FinancialRecordStatus.Paid, record.Status);
        Assert.Equal(1, record.Installment);
        Assert.Equal(paymentDate, record.PaymentDate);
        Assert.Equal(250.00m, record.PaidValue);
        Assert.Equal("12345678901234567890", record.DigitableLine);
        Assert.True(record.Active);
    }

    // -------------------------------------------------------------------------
    // Deactivate — exclusão lógica
    // -------------------------------------------------------------------------

    [Fact]
    public void Deactivate_ShouldSetActiveToFalse()
    {
        var record = new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus);

        record.Deactivate();

        Assert.False(record.Active);
    }

    // -------------------------------------------------------------------------
    // UpdatePaidValue — validações na alteração
    // -------------------------------------------------------------------------

    [Fact]
    public void UpdatePaidValue_WhenPaidValueIsZero_ShouldThrowDomainException()
    {
        var record = new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus);

        var exception = Assert.Throws<DomainException>(() =>
            record.UpdatePaidValue(0m));

        Assert.Equal(FinancialRecordMessages.Errors.PaidValueInvalid, exception.Message);
    }

    [Fact]
    public void UpdatePaidValue_WhenPaidValueIsNegative_ShouldThrowDomainException()
    {
        var record = new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus);

        var exception = Assert.Throws<DomainException>(() =>
            record.UpdatePaidValue(-5m));

        Assert.Equal(FinancialRecordMessages.Errors.PaidValueInvalid, exception.Message);
    }

    [Fact]
    public void UpdatePaidValue_WhenPaidValueIsLessThanValue_ShouldThrowDomainException()
    {
        var record = new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus);

        var exception = Assert.Throws<DomainException>(() =>
            record.UpdatePaidValue(ValidValue - 0.01m));

        Assert.Equal(FinancialRecordMessages.Errors.PaidValuePartialNotAllowed, exception.Message);
    }

    [Fact]
    public void UpdatePaidValue_WhenPaidValueEqualsValue_ShouldUpdateSuccessfully()
    {
        var record = new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus);

        var exception = Record.Exception(() => record.UpdatePaidValue(ValidValue));

        Assert.Null(exception);
        Assert.Equal(ValidValue, record.PaidValue);
    }

    [Fact]
    public void UpdatePaidValue_WhenPaymentDateIsNull_ShouldDefaultToToday()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var record = new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus);

        record.UpdatePaidValue(ValidValue, paymentDate: null);

        Assert.Equal(today, record.PaymentDate);
    }

    [Fact]
    public void UpdatePaidValue_WhenPaymentDateIsProvided_ShouldUseProvidedDate()
    {
        var specificDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var record = new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, ValidTotalInstallment, ValidStatus);

        record.UpdatePaidValue(ValidValue, paymentDate: specificDate);

        Assert.Equal(specificDate, record.PaymentDate);
    }

    // -------------------------------------------------------------------------
    // MarkAsOverdue — atualização de status
    // -------------------------------------------------------------------------

    [Fact]
    public void MarkAsOverdue_ShouldSetStatusToOverdue()
    {
        var record = new FinancialRecordEntity(ValidDescription, ValidValue, ValidDueDate, ValidTotalInstallment, FinancialRecordStatus.Pending);

        record.MarkAsOverdue();

        Assert.Equal(FinancialRecordStatus.Overdue, record.Status);
    }
}
