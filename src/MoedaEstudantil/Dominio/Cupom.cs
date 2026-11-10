using System.Security.Cryptography;

namespace MoedaEstudantil.Dominio;

public class Cupom
{
    private const string Alfabeto = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public long Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public long TransacaoId { get; set; }
    public Transacao Transacao { get; set; } = null!;
    public long AlunoId { get; set; }
    public Aluno Aluno { get; set; } = null!;
    public long VantagemId { get; set; }
    public Vantagem Vantagem { get; set; } = null!;
    public long EmpresaId { get; set; }
    public EmpresaParceira Empresa { get; set; } = null!;
    public DateTime DataGeracao { get; set; }
    public bool Utilizado { get; set; }
    public DateTime? DataUtilizacao { get; set; }

    public static Cupom Emitir(Transacao resgate, string codigo)
    {
        if (resgate.Tipo != TipoTransacao.Resgate || resgate.Vantagem is null)
            throw new RegraDeNegocioException("O cupom só é gerado no resgate de uma vantagem.");

        var cupom = new Cupom
        {
            Codigo = codigo,
            Transacao = resgate,
            Aluno = resgate.Aluno,
            Vantagem = resgate.Vantagem,
            EmpresaId = resgate.Vantagem.EmpresaId,
            Empresa = resgate.Vantagem.Empresa,
            DataGeracao = resgate.DataHora
        };
        resgate.Cupom = cupom;
        resgate.Aluno.Cupons.Add(cupom);
        return cupom;
    }

    public static string GerarCodigoUnico()
    {
        var letras = RandomNumberGenerator.GetString(Alfabeto, 8);
        return $"ME-{letras[..4]}-{letras[4..]}";
    }

    public static string NormalizarCodigo(string? codigo)
        => (codigo ?? string.Empty).Trim().ToUpperInvariant();

    public bool Validar(string? codigo)
        => !Utilizado && string.Equals(Codigo, NormalizarCodigo(codigo), StringComparison.Ordinal);

    public void MarcarUtilizado(DateTime dataUtilizacao)
    {
        if (Utilizado)
            throw new RegraDeNegocioException(
                $"O cupom {Codigo} já foi utilizado em {DataUtilizacao:dd/MM/yyyy 'às' HH:mm}.");

        Utilizado = true;
        DataUtilizacao = dataUtilizacao;
    }
}
