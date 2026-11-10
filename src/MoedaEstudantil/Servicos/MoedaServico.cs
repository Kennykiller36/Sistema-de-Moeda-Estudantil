using Microsoft.EntityFrameworkCore;
using MoedaEstudantil.Dominio;
using MoedaEstudantil.Persistencia;
using MoedaEstudantil.Persistencia.Repositorios;

namespace MoedaEstudantil.Servicos;

public class MoedaServico
{
    private readonly IProfessorRepositorio _professores;
    private readonly IAlunoRepositorio _alunos;
    private readonly ITransacaoRepositorio _transacoes;
    private readonly IUnidadeDeTrabalho _unidade;
    private readonly NotificacaoServico _notificacoes;
    private readonly TimeProvider _relogio;

    public MoedaServico(
        IProfessorRepositorio professores,
        IAlunoRepositorio alunos,
        ITransacaoRepositorio transacoes,
        IUnidadeDeTrabalho unidade,
        NotificacaoServico notificacoes,
        TimeProvider relogio)
    {
        _professores = professores;
        _alunos = alunos;
        _transacoes = transacoes;
        _unidade = unidade;
        _notificacoes = notificacoes;
        _relogio = relogio;
    }

    public string SemestreAtual => CreditoSemestral.SemestreDe(_relogio.Agora());

    public async Task<int> CreditarSemestreDeTodosAsync(CancellationToken cancelamento = default)
    {
        var semestre = SemestreAtual;
        var professores = await _professores.ListarSemCreditoAsync(semestre, cancelamento);
        foreach (var professor in professores)
            await _professores.AdicionarCreditoAsync(professor.CreditarSemestre(semestre, _relogio.Agora()), cancelamento);

        if (professores.Count > 0)
            await _unidade.SalvarAsync(cancelamento);
        return professores.Count;
    }

    public async Task CreditarSemestreAsync(long professorId, CancellationToken cancelamento = default)
    {
        var semestre = SemestreAtual;
        if (await _professores.PossuiCreditoAsync(professorId, semestre, cancelamento))
            return;

        var professor = await _professores.ObterParaEdicaoAsync(professorId, cancelamento);
        if (professor is null)
            return;

        await _professores.AdicionarCreditoAsync(professor.CreditarSemestre(semestre, _relogio.Agora()), cancelamento);
        try
        {
            await _unidade.SalvarAsync(cancelamento);
        }
        catch (DbUpdateException)
        {
            // Outra requisição aplicou o crédito primeiro; o índice único (professor, semestre) barrou a duplicidade.
            _unidade.DescartarAlteracoes();
        }
    }

    public async Task<Resultado<Transacao>> EnviarAsync(
        long professorId,
        long alunoId,
        int quantidade,
        string? motivo,
        CancellationToken cancelamento = default)
    {
        await CreditarSemestreAsync(professorId, cancelamento);

        var professor = await _professores.ObterParaEdicaoAsync(professorId, cancelamento);
        if (professor is null)
            return Resultado<Transacao>.Ausente();

        var aluno = await _alunos.ObterParaEdicaoAsync(alunoId, cancelamento);
        if (aluno is null)
            return Resultado<Transacao>.Falha("Selecione um aluno cadastrado.");

        Transacao envio;
        try
        {
            envio = professor.EnviarMoedas(aluno, quantidade, motivo, _relogio.Agora());
        }
        catch (RegraDeNegocioException erro)
        {
            return Resultado<Transacao>.Falha(erro.Message);
        }

        await _transacoes.AdicionarAsync(envio, cancelamento);
        try
        {
            await _unidade.SalvarAsync(cancelamento);
        }
        catch (DbUpdateConcurrencyException)
        {
            _unidade.DescartarAlteracoes();
            return Resultado<Transacao>.Falha("O saldo mudou durante o envio. Confira o extrato e tente de novo.");
        }

        var avisado = await _notificacoes.NotificarMoedasRecebidasAsync(envio, cancelamento);
        return Resultado<Transacao>.Ok(envio, avisado
            ? null
            : "As moedas foram enviadas, mas o e-mail para o aluno não saiu. Verifique a configuração de e-mail.");
    }

    public Transacao DescontarResgate(Aluno aluno, Vantagem vantagem)
        => aluno.ResgatarVantagem(vantagem, _relogio.Agora());
}
