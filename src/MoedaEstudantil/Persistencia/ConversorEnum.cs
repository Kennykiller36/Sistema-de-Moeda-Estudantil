using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MoedaEstudantil.Dominio;

namespace MoedaEstudantil.Persistencia;

internal static class ConversorEnum
{
    public static ValueConverter<TEnum, string> Texto<TEnum>() where TEnum : struct, Enum
        => new(
            valor => valor.ToString()!.ToUpperInvariant(),
            texto => Enum.Parse<TEnum>(texto, true));
}
