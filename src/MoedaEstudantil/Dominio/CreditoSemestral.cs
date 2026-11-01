namespace MoedaEstudantil.Dominio;

public class CreditoSemestral
{
    public long Id { get; set; }
    public long ProfessorId { get; set; }
    public Professor Professor { get; set; } = null!;
    public string Semestre { get; set; } = string.Empty;
    public int Quantidade { get; set; } = 1000;
}
