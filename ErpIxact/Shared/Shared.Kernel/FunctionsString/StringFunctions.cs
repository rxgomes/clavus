namespace Shared.Kernel.FunctionsString;

public static class StringFunctions
{
    public static string ExtractDigits(string value) =>
        new(value.Where(char.IsDigit).ToArray());

    public static string FormatCpf(string cpf)
    {
        var digits = ExtractDigits(cpf);
        return $"{digits[..3]}.{digits[3..6]}.{digits[6..9]}-{digits[9..11]}";
    }

    public static string FormatCnpj(string cnpj)
    {
        var digits = ExtractDigits(cnpj);
        return $"{digits[..2]}.{digits[2..5]}.{digits[5..8]}/{digits[8..12]}-{digits[12..14]}";
    }
}