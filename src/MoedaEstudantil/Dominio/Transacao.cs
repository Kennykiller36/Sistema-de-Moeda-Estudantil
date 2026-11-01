namespace MoedaEstudantil.Dominio;

public class Transacao
{
    public long Id { get; set; }
    public long AlunoId { get; set; }
    public Aluno Aluno { get; set; } = null!;
    public long? ProfessorId { get; set; }
    public Professor? Professor { get; set; }
    public long? VantagemId { get; set; }
    public Vantagem? Vantagem { get; set; }
    public TipoTransacao Tipo { get; set; }
    public int Quantidade { get; set; }
    public string? Motivo { get; set; }
    public DateTime DataHora { get; set; }

    public Cupom? Cupom { get; set; }
    public ICollection<Notificacao> Notificacoes { get; set; } = new List<Notificacao>();
}
