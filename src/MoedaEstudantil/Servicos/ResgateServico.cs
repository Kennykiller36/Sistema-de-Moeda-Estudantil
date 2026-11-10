using Microsoft.EntityFrameworkCore;
using MoedaEstudantil.Dominio;
using MoedaEstudantil.Persistencia;
using MoedaEstudantil.Persistencia.Repositorios;

namespace MoedaEstudantil.Servicos;

public class ResgateServico
{
    private const int TentativasDeCodigo = 5;

    private readonly IAlunoRepositorio _alunos;
    private readonly IVantagemRepositorio _vantagens;
    private readonly ITransacaoRepositorio _transacoes;
    private readonly ICupomRepositorio _cupons;
    private readonly IUnidadeDeTrabalho _unidade;
    private readonly MoedaServico _moedas;
    private readonly NotificacaoServico _notificacoes;
    private readonly TimeProvider _relogio;

    public ResgateServico(
        IAlunoRepositorio alunos,
        IVantagemRepositorio vantagens,
        ITransacaoRepositorio transacoes,
        ICupomRepositorio cupons,
        IUnidadeDeTrabalho unidade,
        MoedaServico moedas,
        NotificacaoServico notificacoes,
        TimeProvider relogio)
    {
        _alunos = alunos;
        _vantagens = vantagens;
        _transacoes = transacoes;
        _cupons = cupons;
        _unidade = unidade;
        _moedas = moedas;
        _notificacoes = notificacoes;
        _relogio = relogio;
    }

    public async Task<Resultado<Cupom>> ResgatarAsync(long alunoId, long vantagemId, CancellationToken cancelamento = default)
    {
        var aluno = await _alunos.ObterParaEdicaoAsync(alunoId, cancelamento);
        var vantagem = await _vantagens.ObterParaEdicaoAsync(vantagemId, cancelamento);
        if (aluno is null || vantagem is null)
            return Resultado<Cupom>.Ausente();

        Transacao resgate;
        try
        {
            resgate = _moedas.DescontarResgate(aluno, vantagem);
        }
        catch (RegraDeNegocioException erro)
        {
            return Resultado<Cupom>.Falha(erro.Message);
        }

        var cupom = Cupom.Emitir(resgate, await GerarCodigoAsync(cancelamento));
        await _transacoes.AdicionarAsync(resgate, cancelamento);
        await _cupons.AdicionarAsync(cupom, cancelamento);
        try
        {
            await _unidade.SalvarAsync(cancelamento);
        }
        catch (DbUpdateConcurrencyException)
        {
            _unidade.DescartarAlteracoes();
            return Resultado<Cupom>.Falha("O seu saldo mudou durante a troca. Confira o extrato e tente de novo.");
        }

        var avisados = await _notificacoes.NotificarResgateAsync(cupom, cancelamento);
        return Resultado<Cupom>.Ok(cupom, avisados
            ? null
            : "A troca foi registrada, mas algum e-mail do cupom não saiu. O código abaixo continua válido.");
    }

    public Task<Cupom?> ObterCupomDoAlunoAsync(long alunoId, string codigo, CancellationToken cancelamento = default)
        => _cupons.ObterDoAlunoAsync(alunoId, Cupom.NormalizarCodigo(codigo), cancelamento);

    public Task<IReadOnlyList<Cupom>> ListarCuponsDaEmpresaAsync(long empresaId, CancellationToken cancelamento = default)
        => _cupons.ListarDaEmpresaAsync(empresaId, cancelamento);

    public Task<Cupom?> LocalizarCupomAsync(long empresaId, string? codigo, CancellationToken cancelamento = default)
        => _cupons.ObterDaEmpresaAsync(empresaId, Cupom.NormalizarCodigo(codigo), cancelamento);

    public async Task<Resultado<Cupom>> ConfirmarUtilizacaoAsync(long empresaId, string? codigo, CancellationToken cancelamento = default)
    {
        var cupom = await LocalizarCupomAsync(empresaId, codigo, cancelamento);
        if (cupom is null)
            return Resultado<Cupom>.Falha("Nenhum cupom da sua empresa tem esse código.");

        try
        {
            cupom.Empresa.ConferirCupom(cupom, codigo ?? string.Empty, _relogio.Agora());
        }
        catch (RegraDeNegocioException erro)
        {
            return Resultado<Cupom>.Falha(erro.Message);
        }

        try
        {
            await _unidade.SalvarAsync(cancelamento);
        }
        catch (DbUpdateConcurrencyException)
        {
            _unidade.DescartarAlteracoes();
            return Resultado<Cupom>.Falha("Este cupom acabou de ser confirmado em outro atendimento.");
        }

        return Resultado<Cupom>.Ok(cupom);
    }

    private async Task<string> GerarCodigoAsync(CancellationToken cancelamento)
    {
        for (var tentativa = 0; tentativa < TentativasDeCodigo; tentativa++)
        {
            var codigo = Cupom.GerarCodigoUnico();
            if (!await _cupons.CodigoEmUsoAsync(codigo, cancelamento))
                return codigo;
        }

        throw new InvalidOperationException("Não foi possível gerar um código de cupom único.");
    }
}
