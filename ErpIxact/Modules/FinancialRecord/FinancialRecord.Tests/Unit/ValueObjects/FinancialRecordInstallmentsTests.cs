using FinancialRecord.Domain.Enums;
using FinancialRecord.Domain.Exceptions;
using FinancialRecord.Domain.Messages;

namespace FinancialRecord.Tests.Unit.ValueObjects;

public class FinancialRecordInstallmentsTests
{
    private static readonly string ValidDescription = "Parcela de financiamento";
    private static readonly decimal ValidValue = 500.00m;
    private static readonly DateOnly ValidDueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));
    private static readonly FinancialRecordStatus ValidStatus = FinancialRecordStatus.Pending;

    // -------------------------------------------------------------------------
    // TotalInstallment = 1 — cria um único registro com Installment = 1
    // -------------------------------------------------------------------------

    [Fact]
    public void CreateInstallments_WhenTotalInstallmentIsOne_ShouldReturnSingleRecord()
    {
        var records = FinancialRecordEntity.CreateInstallments(
            ValidDescription, ValidValue, ValidDueDate, totalInstallment: 1, ValidStatus);

        Assert.Single(records);
    }

    [Fact]
    public void CreateInstallments_WhenTotalInstallmentIsOne_ShouldSetInstallmentToOne()
    {
        var records = FinancialRecordEntity.CreateInstallments(
            ValidDescription, ValidValue, ValidDueDate, totalInstallment: 1, ValidStatus);

        Assert.Equal(1, records[0].Installment);
    }

    [Fact]
    public void CreateInstallments_WhenTotalInstallmentIsOne_ShouldSetDueDateToProvided()
    {
        var records = FinancialRecordEntity.CreateInstallments(
            ValidDescription, ValidValue, ValidDueDate, totalInstallment: 1, ValidStatus);

        Assert.Equal(ValidDueDate, records[0].DueDate);
    }

    [Fact]
    public void CreateInstallments_WhenTotalInstallmentIsOne_ShouldSetValueToProvided()
    {
        var records = FinancialRecordEntity.CreateInstallments(
            ValidDescription, ValidValue, ValidDueDate, totalInstallment: 1, ValidStatus);

        Assert.Equal(ValidValue, records[0].Value);
    }

    // -------------------------------------------------------------------------
    // TotalInstallment > 1 — cria N registros com Installment de 1 a N
    // -------------------------------------------------------------------------

    [Fact]
    public void CreateInstallments_WhenTotalInstallmentIsThree_ShouldReturnThreeRecords()
    {
        var records = FinancialRecordEntity.CreateInstallments(
            ValidDescription, ValidValue, ValidDueDate, totalInstallment: 3, ValidStatus);

        Assert.Equal(3, records.Count);
    }

    [Fact]
    public void CreateInstallments_WhenTotalInstallmentIsThree_ShouldNumberInstallmentsSequentially()
    {
        var records = FinancialRecordEntity.CreateInstallments(
            ValidDescription, ValidValue, ValidDueDate, totalInstallment: 3, ValidStatus);

        Assert.Equal(1, records[0].Installment);
        Assert.Equal(2, records[1].Installment);
        Assert.Equal(3, records[2].Installment);
    }

    [Fact]
    public void CreateInstallments_WhenTotalInstallmentIsThree_FirstRecordShouldHaveProvidedDueDate()
    {
        var records = FinancialRecordEntity.CreateInstallments(
            ValidDescription, ValidValue, ValidDueDate, totalInstallment: 3, ValidStatus);

        Assert.Equal(ValidDueDate, records[0].DueDate);
    }

    [Fact]
    public void CreateInstallments_WhenTotalInstallmentIsThree_SubsequentDueDatesShouldBeOneMonthApart()
    {
        var records = FinancialRecordEntity.CreateInstallments(
            ValidDescription, ValidValue, ValidDueDate, totalInstallment: 3, ValidStatus);

        Assert.Equal(ValidDueDate.AddMonths(1), records[1].DueDate);
        Assert.Equal(ValidDueDate.AddMonths(2), records[2].DueDate);
    }

    [Fact]
    public void CreateInstallments_WhenTotalInstallmentIsThree_AllRecordsShouldHaveSameValue()
    {
        var records = FinancialRecordEntity.CreateInstallments(
            ValidDescription, ValidValue, ValidDueDate, totalInstallment: 3, ValidStatus);

        Assert.All(records, r => Assert.Equal(ValidValue, r.Value));
    }

    [Fact]
    public void CreateInstallments_WhenTotalInstallmentIsThree_AllRecordsShouldHaveSameDescription()
    {
        var records = FinancialRecordEntity.CreateInstallments(
            ValidDescription, ValidValue, ValidDueDate, totalInstallment: 3, ValidStatus);

        Assert.All(records, r => Assert.Equal(ValidDescription, r.Description));
    }

    [Fact]
    public void CreateInstallments_WhenTotalInstallmentIsThree_AllRecordsShouldHaveSameTotalInstallment()
    {
        var records = FinancialRecordEntity.CreateInstallments(
            ValidDescription, ValidValue, ValidDueDate, totalInstallment: 3, ValidStatus);

        Assert.All(records, r => Assert.Equal(3, r.TotalInstallment));
    }

    [Fact]
    public void CreateInstallments_WhenTotalInstallmentIsThree_AllRecordsShouldBeActive()
    {
        var records = FinancialRecordEntity.CreateInstallments(
            ValidDescription, ValidValue, ValidDueDate, totalInstallment: 3, ValidStatus);

        Assert.All(records, r => Assert.True(r.Active));
    }

    [Fact]
    public void CreateInstallments_WhenTotalInstallmentIsThree_AllRecordsShouldHaveUniqueIds()
    {
        var records = FinancialRecordEntity.CreateInstallments(
            ValidDescription, ValidValue, ValidDueDate, totalInstallment: 3, ValidStatus);

        var ids = records.Select(r => r.Id).Distinct();
        Assert.Equal(3, ids.Count());
    }

    // -------------------------------------------------------------------------
    // Validação propagada — regras da entidade devem ser aplicadas no factory
    // -------------------------------------------------------------------------

    [Fact]
    public void CreateInstallments_WhenTotalInstallmentIsZero_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            FinancialRecordEntity.CreateInstallments(
                ValidDescription, ValidValue, ValidDueDate, totalInstallment: 0, ValidStatus));

        Assert.Equal(FinancialRecordMessages.Errors.TotalInstallmentInvalid, exception.Message);
    }

    [Fact]
    public void CreateInstallments_WhenDescriptionIsNull_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            FinancialRecordEntity.CreateInstallments(
                null!, ValidValue, ValidDueDate, totalInstallment: 1, ValidStatus));

        Assert.Equal(FinancialRecordMessages.Errors.DescriptionRequired, exception.Message);
    }

    [Fact]
    public void CreateInstallments_WhenValueIsZero_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            FinancialRecordEntity.CreateInstallments(
                ValidDescription, 0m, ValidDueDate, totalInstallment: 1, ValidStatus));

        Assert.Equal(FinancialRecordMessages.Errors.ValueRequired, exception.Message);
    }

    // -------------------------------------------------------------------------
    // Cenário com 12 parcelas — smoke test para garantir a lógica mensal
    // -------------------------------------------------------------------------

    [Fact]
    public void CreateInstallments_WhenTotalInstallmentIsTwelve_ShouldCreate12RecordsWithCorrectDueDates()
    {
        var records = FinancialRecordEntity.CreateInstallments(
            ValidDescription, ValidValue, ValidDueDate, totalInstallment: 12, ValidStatus);

        Assert.Equal(12, records.Count);

        for (var i = 0; i < 12; i++)
        {
            Assert.Equal(i + 1, records[i].Installment);
            Assert.Equal(ValidDueDate.AddMonths(i), records[i].DueDate);
        }
    }
}
