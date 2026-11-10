using MoedaEstudantil.Dominio;
using MoedaEstudantil.Persistencia;
using MoedaEstudantil.Persistencia.Repositorios;

namespace MoedaEstudantil.Servicos;

public class ProfessorServico
{
    private readonly IProfessorRepositorio _professores;
    private readonly IAlunoRepositorio _alunos;
    private readonly ITransacaoRepositorio _transacoes;
    private readonly IUnidadeDeTrabalho _unidade;
    private readonly MoedaServico _moedas;

    public ProfessorServico(
        IProfessorRepositorio professores,
        IAlunoRepositorio alunos,
        ITransacaoRepositorio transacoes,
        IUnidadeDeTrabalho unidade,
        MoedaServico moedas)
    {
        _professores = professores;
        _alunos = alunos;
        _transacoes = transacoes;
        _unidade = unidade;
        _moedas = moedas;
    }

    public async Task<Professor?> ObterAsync(long id, CancellationToken cancelamento = default)
    {
        await _moedas.CreditarSemestreAsync(id, cancelamento);
        return await _professores.ObterAsync(id, cancelamento);
    }

    public async Task<IReadOnlyList<Aluno>> ListarAlunosAsync(long professorId, CancellationToken cancelamento = default)
    {
        var professor = await _professores.ObterAsync(professorId, cancelamento);
        return professor is null
            ? []
            : await _alunos.ListarDaInstituicaoAsync(professor.InstituicaoId, cancelamento);
    }

    public async Task<Extrato?> ExtratoAsync(long professorId, CancellationToken cancelamento = default)
    {
        var professor = await ObterAsync(professorId, cancelamento);
        if (professor is null)
            return null;

        var envios = await _transacoes.ListarEnviosDoProfessorAsync(professorId, cancelamento);
        var creditos = await _professores.ListarCreditosAsync(professorId, cancelamento);
        var movimentos = envios
            .Select(envio => new MovimentoExtrato(
                envio.DataHora, "Envio", $"Para {envio.Aluno.Nome}", envio.Motivo, -envio.Quantidade))
            .Concat(creditos.Select(credito => new MovimentoExtrato(
                credito.DataAplicacao, "Crédito semestral", $"Semestre {credito.Semestre}",
                "Somado ao saldo corrente", credito.Quantidade)));

        return new Extrato(professor.Nome, professor.Saldo, movimentos);
    }

    public async Task<ResultadoCadastro> AlterarSenhaAsync(
        long professorId,
        string senhaAtual,
        string novaSenha,
        CancellationToken cancelamento = default)
    {
        var professor = await _professores.ObterParaEdicaoAsync(professorId, cancelamento);
        if (professor is null)
            return ResultadoCadastro.Ausente();

        if (!SenhaHasher.Verificar(senhaAtual, professor.Senha))
            return ResultadoCadastro.Falha(new Dictionary<string, string> { ["SenhaAtual"] = "A senha atual não confere." });
        if (novaSenha.Length < 6)
            return ResultadoCadastro.Falha(new Dictionary<string, string> { ["NovaSenha"] = "A senha deve ter pelo menos 6 caracteres." });

        professor.Senha = SenhaHasher.Gerar(novaSenha);
        await _unidade.SalvarAsync(cancelamento);
        return ResultadoCadastro.Ok(professorId);
    }
}
