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
}
