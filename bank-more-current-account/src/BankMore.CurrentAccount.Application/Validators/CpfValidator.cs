namespace BankMore.CurrentAccount.Application.Validators;

public static class CpfValidator
{
    public static string Normalize(string cpf)
    {
        return new string(cpf.Where(char.IsDigit).ToArray());
    }

    public static bool IsValid(string cpf)
    {
        var digits = Normalize(cpf);
        if (digits.Length != 11) return false;
        if (digits.Distinct().Count() == 1) return false;

        int[] weights1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] weights2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        var sum = 0;
        for (var i = 0; i < 9; i++)
            sum += (digits[i] - '0') * weights1[i];
        var remainder = sum % 11;
        var digit1 = remainder < 2 ? 0 : 11 - remainder;
        if (digits[9] - '0' != digit1) return false;

        sum = 0;
        for (var i = 0; i < 10; i++)
            sum += (digits[i] - '0') * weights2[i];
        remainder = sum % 11;
        var digit2 = remainder < 2 ? 0 : 11 - remainder;
        return digits[10] - '0' == digit2;
    }
}
