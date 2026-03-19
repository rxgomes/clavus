using Shared.Kernel.FunctionsString;

namespace Shared.Kernel.ValueObjects;

public sealed class DocNumber
{
    public string Value { get; }

    public DocNumber(string value)
    {
        var digits = StringFunctions.ExtractDigits(value);

        var isValid = digits.Length switch
        {
            11 => ValidateCpf(digits),
            14 => ValidateCnpj(digits),
            _ => false,
        };

        if (!isValid)
        {
            throw new ArgumentException($"DocNumber inválido: '{value}'.");
        }
        
        Value = digits;
    }

    private static bool ValidateCpf(string cpf)
    {
        if (cpf.Distinct().Count() == 1)
        {
            return false;
        }

        int sum = 0;
        for (int i = 0; i < 9; i++)
            sum += (cpf[i] - '0') * (10 - i);

        int remainder = sum % 11;
        int digit1 = remainder < 2 ? 0 : 11 - remainder;
        if (digit1 != cpf[9] - '0')
        {
            return false;
        }

        sum = 0;
        for (int i = 0; i < 10; i++)
            sum += (cpf[i] - '0') * (11 - i);

        remainder = sum % 11;
        int digit2 = remainder < 2 ? 0 : 11 - remainder;
        return digit2 == cpf[10] - '0';
    }

    private static readonly int[] CnpjWeights1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] CnpjWeights2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    private static bool ValidateCnpj(string cnpj)
    {
        if (cnpj.Distinct().Count() == 1)
        {
            return false;
        }

        int sum = 0;
        for (int i = 0; i < 12; i++)
            sum += (cnpj[i] - '0') * CnpjWeights1[i];

        int remainder = sum % 11;
        int digit1 = remainder < 2 ? 0 : 11 - remainder;
        if (digit1 != cnpj[12] - '0')
        {
            return false;
        }

        sum = 0;
        for (int i = 0; i < 13; i++)
            sum += (cnpj[i] - '0') * CnpjWeights2[i];

        remainder = sum % 11;
        int digit2 = remainder < 2 ? 0 : 11 - remainder;
        return digit2 == cnpj[13] - '0';
    }

    public bool IsCpf => Value.Length == 11;
    public bool IsCnpj => Value.Length == 14;

    public override string ToString() => Value;

    public override bool Equals(object? obj) => obj is DocNumber other && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(DocNumber? left, DocNumber? right) => left?.Value == right?.Value;

    public static bool operator !=(DocNumber? left, DocNumber? right) => !(left == right);
}
