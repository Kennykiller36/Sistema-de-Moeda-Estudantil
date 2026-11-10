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

    public void ReceberMoedas(int quantidade)
    {
        if (quantidade <= 0)
            throw new RegraDeNegocioException("A quantidade recebida precisa ser maior que zero.");

        Saldo += quantidade;
    }

    public Transacao ResgatarVantagem(Vantagem vantagem, DateTime dataHora)
    {
        if (!vantagem.Ativa)
            throw new RegraDeNegocioException("Esta vantagem não está mais disponível.");
        if (vantagem.CustoMoedas > Saldo)
            throw new RegraDeNegocioException(
                $"Saldo insuficiente: a vantagem custa {vantagem.CustoMoedas} moedas e você tem {Saldo}.");

        Saldo -= vantagem.CustoMoedas;

        var resgate = new Transacao
        {
            Aluno = this,
            Vantagem = vantagem,
            Tipo = TipoTransacao.Resgate,
            Quantidade = vantagem.CustoMoedas,
            DataHora = dataHora
        };
        Transacoes.Add(resgate);
        return resgate;
    }
}
