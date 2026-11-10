using MoedaEstudantil.Dominio;
using MoedaEstudantil.Externos;
using MoedaEstudantil.Persistencia;
using MoedaEstudantil.Persistencia.Repositorios;

namespace MoedaEstudantil.Servicos;

public class NotificacaoServico
{
    private readonly IServicoEmail _email;
    private readonly INotificacaoRepositorio _notificacoes;
    private readonly IUnidadeDeTrabalho _unidade;
    private readonly TimeProvider _relogio;
    private readonly ILogger<NotificacaoServico> _log;

    public NotificacaoServico(
        IServicoEmail email,
        INotificacaoRepositorio notificacoes,
        IUnidadeDeTrabalho unidade,
        TimeProvider relogio,
        ILogger<NotificacaoServico> log)
    {
        _email = email;
        _notificacoes = notificacoes;
        _unidade = unidade;
        _relogio = relogio;
        _log = log;
    }

    public Task<IReadOnlyList<Notificacao>> ListarAsync(string email, CancellationToken cancelamento = default)
        => _notificacoes.ListarDoDestinatarioAsync(Normalizacao.Email(email), cancelamento);

    public Task<bool> NotificarMoedasRecebidasAsync(Transacao envio, CancellationToken cancelamento = default)
    {
        var aluno = envio.Aluno;
        var professor = envio.Professor!;
        var texto = $"""
            Olá, {aluno.Nome}.

            {professor.Nome} ({professor.Departamento}) enviou {envio.Quantidade} moedas para você.
            Motivo: {envio.Motivo}

            Seu saldo agora é de {aluno.Saldo} moedas. Consulte o extrato ou troque as moedas por vantagens no sistema.

            Moeda Estudantil
            """;

        return EnviarAsync(envio, aluno.Email, TipoNotificacao.Moedas,
            $"Você recebeu {envio.Quantidade} moedas", texto, destaque: null, cancelamento);
    }

    public async Task<bool> NotificarResgateAsync(Cupom cupom, CancellationToken cancelamento = default)
    {
        var aluno = cupom.Aluno;
        var vantagem = cupom.Vantagem;
        var empresa = cupom.Empresa;

        var textoAluno = $"""
            Olá, {aluno.Nome}.

            Você trocou {vantagem.CustoMoedas} moedas por uma vantagem de {empresa.NomeFantasia}:
            {vantagem.Descricao}

            Apresente o código do cupom na troca presencial: {cupom.Codigo}

            Saldo restante: {aluno.Saldo} moedas.

            Moeda Estudantil
            """;
        var textoEmpresa = $"""
            Olá, {empresa.NomeFantasia}.

            {aluno.Nome} resgatou uma vantagem da sua empresa:
            {vantagem.Descricao} ({vantagem.CustoMoedas} moedas)

            Código do cupom: {cupom.Codigo}

            Na troca presencial, confira o código em Cupons, no sistema, e confirme a utilização.

            Moeda Estudantil
            """;

        var alunoAvisado = await EnviarAsync(cupom.Transacao, aluno.Email, TipoNotificacao.Cupom,
            $"Seu cupom {cupom.Codigo}", textoAluno, cupom.Codigo, cancelamento);
        var empresaAvisada = await EnviarAsync(cupom.Transacao, empresa.Email, TipoNotificacao.Cupom,
            $"Resgate para conferir: cupom {cupom.Codigo}", textoEmpresa, cupom.Codigo, cancelamento);
        return alunoAvisado && empresaAvisada;
    }

    private async Task<bool> EnviarAsync(
        Transacao transacao,
        string destinatario,
        TipoNotificacao tipo,
        string assunto,
        string texto,
        string? destaque,
        CancellationToken cancelamento)
    {
        try
        {
            await _email.EnviarAsync(new MensagemEmail(destinatario, assunto, texto, destaque), cancelamento);
        }
        catch (Exception erro) when (erro is not OperationCanceledException)
        {
            _log.LogError(erro, "Falha ao enviar o e-mail \"{Assunto}\" para {Destinatario}.", assunto, destinatario);
            return false;
        }

        await _notificacoes.AdicionarAsync(new Notificacao
        {
            Transacao = transacao,
            Destinatario = destinatario,
            Tipo = tipo,
            Assunto = assunto,
            Corpo = texto.Length > 4000 ? texto[..4000] : texto,
            DataEnvio = _relogio.Agora()
        }, cancelamento);
        await _unidade.SalvarAsync(cancelamento);
        return true;
    }
}
