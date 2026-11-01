namespace MoedaEstudantil.Dominio;

public class Cupom
{
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
    public bool Utilizado { get; set; }
    public DateTime? DataUtilizacao { get; set; }
}
