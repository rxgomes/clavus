using BankAccount.Domain.Exceptions;
using BankAccount.Domain.Messages;
using Xunit;
using BankAccountEntity = BankAccount.Domain.Entities.BankAccount;

namespace BankAccount.Tests.Unit.Entities;

public class BankAccountEntityTests
{
    private const string ValidNameBank = "Banco do Brasil";
    private const string ValidNumberAccount = "123456789";
    private const string ValidDigitAccount = "1";

    // --- Validações de NameBank ---

    [Fact]
    public void Constructor_WhenNameBankIsEmpty_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new BankAccountEntity(string.Empty, ValidNumberAccount, ValidDigitAccount));

        Assert.Equal(BankAccountMessages.Errors.NameBankRequired, exception.Message);
    }

    [Fact]
    public void Constructor_WhenNameBankIsWhiteSpace_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new BankAccountEntity("   ", ValidNumberAccount, ValidDigitAccount));

        Assert.Equal(BankAccountMessages.Errors.NameBankRequired, exception.Message);
    }

    [Fact]
    public void Constructor_WhenNameBankExceedsMaxLength_ShouldThrowDomainException()
    {
        var nameTooLong = new string('A', 26);

        var exception = Assert.Throws<DomainException>(() =>
            new BankAccountEntity(nameTooLong, ValidNumberAccount, ValidDigitAccount));

        Assert.Equal(BankAccountMessages.Errors.NameBankTooLong, exception.Message);
    }

    [Fact]
    public void Constructor_WhenNameBankIsExactlyMaxLength_ShouldNotThrow()
    {
        var nameExactMax = new string('A', 25);

        var exception = Record.Exception(() =>
            new BankAccountEntity(nameExactMax, ValidNumberAccount, ValidDigitAccount));

        Assert.Null(exception);
    }

    // --- Validações de NumberAccount ---

    [Fact]
    public void Constructor_WhenNumberAccountIsEmpty_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new BankAccountEntity(ValidNameBank, string.Empty, ValidDigitAccount));

        Assert.Equal(BankAccountMessages.Errors.NumberAccountRequired, exception.Message);
    }

    [Fact]
    public void Constructor_WhenNumberAccountIsWhiteSpace_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new BankAccountEntity(ValidNameBank, "   ", ValidDigitAccount));

        Assert.Equal(BankAccountMessages.Errors.NumberAccountRequired, exception.Message);
    }

    [Fact]
    public void Constructor_WhenNumberAccountExceedsMaxLength_ShouldThrowDomainException()
    {
        var numberTooLong = new string('1', 16);

        var exception = Assert.Throws<DomainException>(() =>
            new BankAccountEntity(ValidNameBank, numberTooLong, ValidDigitAccount));

        Assert.Equal(BankAccountMessages.Errors.NumberAccountTooLong, exception.Message);
    }

    [Fact]
    public void Constructor_WhenNumberAccountIsExactlyMaxLength_ShouldNotThrow()
    {
        var numberExactMax = new string('1', 15);

        var exception = Record.Exception(() =>
            new BankAccountEntity(ValidNameBank, numberExactMax, ValidDigitAccount));

        Assert.Null(exception);
    }

    // --- Validações de DigitAccount ---

    [Fact]
    public void Constructor_WhenDigitAccountIsEmpty_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new BankAccountEntity(ValidNameBank, ValidNumberAccount, string.Empty));

        Assert.Equal(BankAccountMessages.Errors.DigitAccountRequired, exception.Message);
    }

    [Fact]
    public void Constructor_WhenDigitAccountIsWhiteSpace_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new BankAccountEntity(ValidNameBank, ValidNumberAccount, "   "));

        Assert.Equal(BankAccountMessages.Errors.DigitAccountRequired, exception.Message);
    }

    [Fact]
    public void Constructor_WhenDigitAccountExceedsMaxLength_ShouldThrowDomainException()
    {
        var digitTooLong = new string('1', 6);

        var exception = Assert.Throws<DomainException>(() =>
            new BankAccountEntity(ValidNameBank, ValidNumberAccount, digitTooLong));

        Assert.Equal(BankAccountMessages.Errors.DigitAccountTooLong, exception.Message);
    }

    [Fact]
    public void Constructor_WhenDigitAccountIsExactlyMaxLength_ShouldNotThrow()
    {
        var digitExactMax = new string('1', 5);

        var exception = Record.Exception(() =>
            new BankAccountEntity(ValidNameBank, ValidNumberAccount, digitExactMax));

        Assert.Null(exception);
    }

    // --- Criação com sucesso ---

    [Fact]
    public void Constructor_WhenAllFieldsAreValid_ShouldCreateBankAccount()
    {
        var bankAccount = new BankAccountEntity(ValidNameBank, ValidNumberAccount, ValidDigitAccount);

        Assert.Equal(ValidNameBank, bankAccount.NameBank);
        Assert.Equal(ValidNumberAccount, bankAccount.NumberAccount);
        Assert.Equal(ValidDigitAccount, bankAccount.DigitAccount);
        Assert.True(bankAccount.Active);
        Assert.NotEqual(Guid.Empty, bankAccount.Id);
        Assert.True(bankAccount.CreatedAt <= DateTime.UtcNow);
        Assert.Null(bankAccount.UpdatedAt);
    }
}
