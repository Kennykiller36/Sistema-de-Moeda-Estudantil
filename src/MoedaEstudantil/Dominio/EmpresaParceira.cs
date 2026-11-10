namespace MoedaEstudantil.Dominio;

public class EmpresaParceira : Usuario
{
    public EmpresaParceira()
    {
        Tipo = TipoUsuario.Empresa;
    }

    public string NomeFantasia { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;

    public ICollection<Vantagem> Vantagens { get; set; } = new List<Vantagem>();
    public ICollection<Cupom> Cupons { get; set; } = new List<Cupom>();

    public void CadastrarVantagem(Vantagem vantagem)
    {
        vantagem.Empresa = this;
        vantagem.Publicar();
        Vantagens.Add(vantagem);
    }

    public void ConferirCupom(Cupom cupom, string codigo, DateTime dataUtilizacao)
    {
        if (cupom.EmpresaId != Id)
            throw new RegraDeNegocioException("Este cupom foi emitido para outra empresa.");
        if (!cupom.Utilizado && !cupom.Validar(codigo))
            throw new RegraDeNegocioException("O código informado não confere com o cupom.");

        cupom.MarcarUtilizado(dataUtilizacao);
    }
}
