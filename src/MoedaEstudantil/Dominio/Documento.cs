namespace MoedaEstudantil.Dominio;

public static class Documento
{
    public static string SomenteDigitos(string? valor)
        => new string((valor ?? string.Empty).Where(char.IsDigit).ToArray());

    public static bool CpfValido(string? valor)
    {
        var cpf = SomenteDigitos(valor);
        if (cpf.Length != 11 || cpf.All(digito => digito == cpf[0]))
            return false;

        return DigitoCpf(cpf[..9]) == cpf[9] - '0'
            && DigitoCpf(cpf[..10]) == cpf[10] - '0';
    }

    public static bool CnpjValido(string? valor)
    {
        var cnpj = SomenteDigitos(valor);
        if (cnpj.Length != 14 || cnpj.All(digito => digito == cnpj[0]))
            return false;

        return DigitoCnpj(cnpj[..12]) == cnpj[12] - '0'
            && DigitoCnpj(cnpj[..13]) == cnpj[13] - '0';
    }

    public static string CompletarCnpj(string base12)
    {
        var baseLimpa = SomenteDigitos(base12);
        if (baseLimpa.Length != 12)
            throw new ArgumentException("A base do CNPJ precisa ter 12 dígitos.", nameof(base12));

        var primeiro = DigitoCnpj(baseLimpa);
        var segundo = DigitoCnpj(baseLimpa + primeiro);
        return baseLimpa + primeiro + segundo;
    }

    public static string FormatarCpf(string? valor)
    {
        var cpf = SomenteDigitos(valor);
        return cpf.Length == 11
            ? $"{cpf[..3]}.{cpf[3..6]}.{cpf[6..9]}-{cpf[9..]}"
            : valor ?? string.Empty;
    }

    public static string FormatarCnpj(string? valor)
    {
        var cnpj = SomenteDigitos(valor);
        return cnpj.Length == 14
            ? $"{cnpj[..2]}.{cnpj[2..5]}.{cnpj[5..8]}/{cnpj[8..12]}-{cnpj[12..]}"
            : valor ?? string.Empty;
    }

    private static int DigitoCpf(string baseNumerica)
    {
        var soma = 0;
        var peso = baseNumerica.Length + 1;
        foreach (var caractere in baseNumerica)
        {
            soma += (caractere - '0') * peso;
            peso--;
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    private static int DigitoCnpj(string baseNumerica)
    {
        var peso = baseNumerica.Length - 7;
        var soma = 0;
        foreach (var caractere in baseNumerica)
        {
            soma += (caractere - '0') * peso;
            peso = peso == 2 ? 9 : peso - 1;
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
