using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Options;

namespace MoedaEstudantil.Externos;

public sealed record MensagemEmail(string Para, string Assunto, string Texto, string? Destaque = null);

public interface IServicoEmail
{
    Task EnviarAsync(MensagemEmail mensagem, CancellationToken cancelamento = default);
}

public class OpcoesEmail
{
    public string Remetente { get; set; } = "nao-responda@moedaestudantil.local";
    public string NomeRemetente { get; set; } = "Moeda Estudantil";
    public string PastaSaida { get; set; } = "dados/emails";
    public string? Host { get; set; }
    public int Porta { get; set; } = 587;
    public bool Ssl { get; set; } = true;
    public string? Usuario { get; set; }
    public string? Senha { get; set; }
}

public class ServicoEmailSmtp : IServicoEmail
{
    private readonly OpcoesEmail _opcoes;

    public ServicoEmailSmtp(IOptions<OpcoesEmail> opcoes) => _opcoes = opcoes.Value;

    public async Task EnviarAsync(MensagemEmail mensagem, CancellationToken cancelamento = default)
    {
        using var email = new MailMessage
        {
            From = new MailAddress(_opcoes.Remetente, _opcoes.NomeRemetente),
            Subject = mensagem.Assunto,
            SubjectEncoding = Encoding.UTF8,
            Body = mensagem.Texto,
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = false
        };
        email.To.Add(mensagem.Para);
        email.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
            ParaHtml(mensagem), Encoding.UTF8, MediaTypeNames.Text.Html));

        using var cliente = CriarCliente();
        await cliente.SendMailAsync(email, cancelamento);
    }

    private SmtpClient CriarCliente()
    {
        if (string.IsNullOrWhiteSpace(_opcoes.Host))
        {
            Directory.CreateDirectory(_opcoes.PastaSaida);
            return new SmtpClient
            {
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                PickupDirectoryLocation = _opcoes.PastaSaida
            };
        }

        var cliente = new SmtpClient(_opcoes.Host, _opcoes.Porta)
        {
            DeliveryMethod = SmtpDeliveryMethod.Network,
            EnableSsl = _opcoes.Ssl
        };
        if (!string.IsNullOrWhiteSpace(_opcoes.Usuario))
            cliente.Credentials = new NetworkCredential(_opcoes.Usuario, _opcoes.Senha);
        return cliente;
    }

    private static string ParaHtml(MensagemEmail mensagem)
    {
        var paragrafos = mensagem.Texto
            .Replace("\r\n", "\n")
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
            .Select(paragrafo => $"<p style=\"margin:0 0 14px\">{WebUtility.HtmlEncode(paragrafo).Replace("\n", "<br>")}</p>");
        var destaque = string.IsNullOrEmpty(mensagem.Destaque)
            ? string.Empty
            : $"<p style=\"margin:4px 0 18px;padding:14px 18px;border:2px dashed #8a6428;font:600 24px Consolas,monospace;letter-spacing:3px;text-align:center\">{WebUtility.HtmlEncode(mensagem.Destaque)}</p>";

        return $"""
            <div style="font-family:'Segoe UI',Arial,sans-serif;font-size:15px;color:#1d2923;max-width:560px">
              <p style="margin:0 0 18px;padding:12px 16px;background:#132e24;color:#f0d7a2;font:600 18px Georgia,serif">Moeda Estudantil</p>
              {destaque}
              {string.Join("\n", paragrafos)}
            </div>
            """;
    }
}
