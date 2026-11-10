namespace MoedaEstudantil.Dominio;

public class Professor : Usuario
{
    public const int TamanhoMaximoMotivo = 500;

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

    public Transacao EnviarMoedas(Aluno aluno, int quantidade, string? motivo, DateTime dataHora)
    {
        if (quantidade <= 0)
            throw new RegraDeNegocioException("Informe uma quantidade de moedas maior que zero.");
        if (string.IsNullOrWhiteSpace(motivo))
            throw new RegraDeNegocioException("Informe o motivo do reconhecimento.");
        if (motivo.Trim().Length > TamanhoMaximoMotivo)
            throw new RegraDeNegocioException($"O motivo pode ter até {TamanhoMaximoMotivo} caracteres.");
        if (aluno.InstituicaoId != InstituicaoId)
            throw new RegraDeNegocioException("O aluno precisa ser da mesma instituição do professor.");
        if (quantidade > Saldo)
            throw new RegraDeNegocioException($"Saldo insuficiente: você tem {Saldo} moedas.");

        Saldo -= quantidade;
        aluno.ReceberMoedas(quantidade);

        var envio = new Transacao
        {
            Aluno = aluno,
            Professor = this,
            Tipo = TipoTransacao.Envio,
            Quantidade = quantidade,
            Motivo = motivo.Trim(),
            DataHora = dataHora
        };
        Transacoes.Add(envio);
        aluno.Transacoes.Add(envio);
        return envio;
    }

    public CreditoSemestral CreditarSemestre(string semestre, DateTime dataAplicacao)
    {
        var credito = new CreditoSemestral
        {
            Professor = this,
            Semestre = semestre,
            DataAplicacao = dataAplicacao
        };
        credito.Aplicar();
        Creditos.Add(credito);
        return credito;
    }
}
