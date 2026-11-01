namespace MoedaEstudantil.Dominio;

public class Professor : Usuario
{
    public Professor()
    {
        Tipo = TipoUsuario.Professor;
    }

    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
    public int Saldo { get; set; }
    public long InstituicaoId { get; set; }
    public Instituicao Instituicao { get; set; } = null!;

    public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
    public ICollection<CreditoSemestral> Creditos { get; set; } = new List<CreditoSemestral>();
}
