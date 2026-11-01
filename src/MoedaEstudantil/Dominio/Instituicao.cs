namespace MoedaEstudantil.Dominio;

public class Instituicao
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;

    public ICollection<Aluno> Alunos { get; set; } = new List<Aluno>();
    public ICollection<Professor> Professores { get; set; } = new List<Professor>();
}
