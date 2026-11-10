namespace MoedaEstudantil.Dominio;

public class Notificacao
{
    public long Id { get; set; }
    public long TransacaoId { get; set; }
    public Transacao Transacao { get; set; } = null!;
    public string Destinatario { get; set; } = string.Empty;
    public TipoNotificacao Tipo { get; set; }
    public string Assunto { get; set; } = string.Empty;
    public string Corpo { get; set; } = string.Empty;
    public DateTime DataEnvio { get; set; }
}
