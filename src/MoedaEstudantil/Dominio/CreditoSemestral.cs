namespace MoedaEstudantil.Dominio;

public class CreditoSemestral
{
    public const int QuantidadePorSemestre = 1000;

    public long Id { get; set; }
    public long ProfessorId { get; set; }
    public Professor Professor { get; set; } = null!;
    public string Semestre { get; set; } = string.Empty;
    public int Quantidade { get; set; } = QuantidadePorSemestre;
    public DateTime DataAplicacao { get; set; }

    public void Aplicar()
    {
        Quantidade = QuantidadePorSemestre;
        Professor.Saldo += Quantidade;
    }

    public static string SemestreDe(DateTime data) => $"{data.Year}/{(data.Month <= 6 ? 1 : 2)}";
}
