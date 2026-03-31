using CreditCard.Domain.Exceptions;
using CreditCard.Domain.Messages;
using Xunit;
using CreditCardEntity = CreditCard.Domain.Entities.CreditCard;

namespace CreditCard.Tests.Unit.Entities;

public class CreditCardEntityTests
{
    private const string ValidName = "Meu Cartão";
    private const string ValidFlag = "Visa";
    private const int ValidCloseDay = 10;
    private const int ValidDueDay = 20;

    // --- Validações de Name ---

    [Fact]
    public void Constructor_WhenNameIsEmpty_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new CreditCardEntity(string.Empty, ValidFlag, ValidCloseDay, ValidDueDay));

        Assert.Equal(CreditCardMessages.Errors.NameRequired, exception.Message);
    }

    [Fact]
    public void Constructor_WhenNameIsWhiteSpace_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new CreditCardEntity("   ", ValidFlag, ValidCloseDay, ValidDueDay));

        Assert.Equal(CreditCardMessages.Errors.NameRequired, exception.Message);
    }

    [Fact]
    public void Constructor_WhenNameExceedsMaxLength_ShouldThrowDomainException()
    {
        var nameTooLong = new string('A', 16);

        var exception = Assert.Throws<DomainException>(() =>
            new CreditCardEntity(nameTooLong, ValidFlag, ValidCloseDay, ValidDueDay));

        Assert.Equal(CreditCardMessages.Errors.NameTooLong, exception.Message);
    }

    [Fact]
    public void Constructor_WhenNameIsExactlyMaxLength_ShouldNotThrow()
    {
        var nameExactMax = new string('A', 15);

        var exception = Record.Exception(() =>
            new CreditCardEntity(nameExactMax, ValidFlag, ValidCloseDay, ValidDueDay));

        Assert.Null(exception);
    }

    // --- Validações de Flag ---

    [Fact]
    public void Constructor_WhenFlagIsEmpty_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new CreditCardEntity(ValidName, string.Empty, ValidCloseDay, ValidDueDay));

        Assert.Equal(CreditCardMessages.Errors.FlagRequired, exception.Message);
    }

    [Fact]
    public void Constructor_WhenFlagIsWhiteSpace_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new CreditCardEntity(ValidName, "   ", ValidCloseDay, ValidDueDay));

        Assert.Equal(CreditCardMessages.Errors.FlagRequired, exception.Message);
    }

    [Fact]
    public void Constructor_WhenFlagExceedsMaxLength_ShouldThrowDomainException()
    {
        var flagTooLong = new string('B', 16);

        var exception = Assert.Throws<DomainException>(() =>
            new CreditCardEntity(ValidName, flagTooLong, ValidCloseDay, ValidDueDay));

        Assert.Equal(CreditCardMessages.Errors.FlagTooLong, exception.Message);
    }

    [Fact]
    public void Constructor_WhenFlagIsExactlyMaxLength_ShouldNotThrow()
    {
        var flagExactMax = new string('B', 15);

        var exception = Record.Exception(() =>
            new CreditCardEntity(ValidName, flagExactMax, ValidCloseDay, ValidDueDay));

        Assert.Null(exception);
    }

    // --- Validações de CloseDay ---

    [Fact]
    public void Constructor_WhenCloseDayIsZero_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new CreditCardEntity(ValidName, ValidFlag, 0, ValidDueDay));

        Assert.Equal(CreditCardMessages.Errors.CloseDayInvalid, exception.Message);
    }

    [Fact]
    public void Constructor_WhenCloseDayIsNegative_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new CreditCardEntity(ValidName, ValidFlag, -1, ValidDueDay));

        Assert.Equal(CreditCardMessages.Errors.CloseDayInvalid, exception.Message);
    }

    [Fact]
    public void Constructor_WhenCloseDayExceedsMaxValue_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new CreditCardEntity(ValidName, ValidFlag, 32, ValidDueDay));

        Assert.Equal(CreditCardMessages.Errors.CloseDayInvalid, exception.Message);
    }

    [Fact]
    public void Constructor_WhenCloseDayIsOne_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
            new CreditCardEntity(ValidName, ValidFlag, 1, ValidDueDay));

        Assert.Null(exception);
    }

    [Fact]
    public void Constructor_WhenCloseDayIsThirtyOne_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
            new CreditCardEntity(ValidName, ValidFlag, 31, ValidDueDay));

        Assert.Null(exception);
    }

    // --- Validações de DueDay ---

    [Fact]
    public void Constructor_WhenDueDayIsZero_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new CreditCardEntity(ValidName, ValidFlag, ValidCloseDay, 0));

        Assert.Equal(CreditCardMessages.Errors.DueDayInvalid, exception.Message);
    }

    [Fact]
    public void Constructor_WhenDueDayIsNegative_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new CreditCardEntity(ValidName, ValidFlag, ValidCloseDay, -1));

        Assert.Equal(CreditCardMessages.Errors.DueDayInvalid, exception.Message);
    }

    [Fact]
    public void Constructor_WhenDueDayExceedsMaxValue_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            new CreditCardEntity(ValidName, ValidFlag, ValidCloseDay, 32));

        Assert.Equal(CreditCardMessages.Errors.DueDayInvalid, exception.Message);
    }

    [Fact]
    public void Constructor_WhenDueDayIsOne_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
            new CreditCardEntity(ValidName, ValidFlag, ValidCloseDay, 1));

        Assert.Null(exception);
    }

    [Fact]
    public void Constructor_WhenDueDayIsThirtyOne_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
            new CreditCardEntity(ValidName, ValidFlag, ValidCloseDay, 31));

        Assert.Null(exception);
    }

    // --- Criação com sucesso ---

    [Fact]
    public void Constructor_WhenAllFieldsAreValid_ShouldCreateCreditCard()
    {
        var card = new CreditCardEntity(ValidName, ValidFlag, ValidCloseDay, ValidDueDay);

        Assert.Equal(ValidName, card.Name);
        Assert.Equal(ValidFlag, card.Flag);
        Assert.Equal(ValidCloseDay, card.CloseDay);
        Assert.Equal(ValidDueDay, card.DueDay);
        Assert.True(card.Active);
        Assert.NotEqual(Guid.Empty, card.Id);
        Assert.True(card.CreatedAt <= DateTime.UtcNow);
        Assert.Null(card.UpdatedAt);
    }
}
