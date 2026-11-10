namespace MoedaEstudantil.Servicos;

public sealed record MovimentoExtrato(
    DateTime Data,
    string Tipo,
    string Descricao,
    string? Detalhe,
    int Quantidade,
    string? CodigoCupom = null,
    bool CupomUtilizado = false);

public sealed record LinhaExtrato(MovimentoExtrato Movimento, int SaldoApos);

public sealed class Extrato
{
    public Extrato(string titular, int saldo, IEnumerable<MovimentoExtrato> movimentos)
    {
        Titular = titular;
        Saldo = saldo;

        var linhas = new List<LinhaExtrato>();
        var saldoApos = saldo;
        foreach (var movimento in movimentos.OrderByDescending(movimento => movimento.Data))
        {
            linhas.Add(new LinhaExtrato(movimento, saldoApos));
            saldoApos -= movimento.Quantidade;
        }

        Linhas = linhas;
    }

    public string Titular { get; }
    public int Saldo { get; }
    public IReadOnlyList<LinhaExtrato> Linhas { get; }

    public int TotalEntradas => Linhas.Sum(linha => Math.Max(linha.Movimento.Quantidade, 0));
    public int TotalSaidas => Linhas.Sum(linha => Math.Max(-linha.Movimento.Quantidade, 0));
}
