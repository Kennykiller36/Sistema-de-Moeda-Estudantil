namespace MoedaEstudantil.Dominio;

public class Aluno : Usuario
{
    public Aluno()
    {
        Tipo = TipoUsuario.Aluno;
    }

    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Rg { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Curso { get; set; } = string.Empty;
    public int Saldo { get; set; }
    public long InstituicaoId { get; set; }
    public Instituicao Instituicao { get; set; } = null!;

    public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
    public ICollection<Cupom> Cupons { get; set; } = new List<Cupom>();
}
