namespace MoedaEstudantil.Dominio;

public class Vantagem
{
    public long Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string Foto { get; set; } = string.Empty;
    public int CustoMoedas { get; set; }
    public bool Ativa { get; set; } = true;
    public long EmpresaId { get; set; }
    public EmpresaParceira Empresa { get; set; } = null!;

    public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
    public ICollection<Cupom> Cupons { get; set; } = new List<Cupom>();

    public void Publicar() => Ativa = true;

    public void Desativar() => Ativa = false;
}
