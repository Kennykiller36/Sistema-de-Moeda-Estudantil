using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MoedaEstudantil.Dominio;
using MoedaEstudantil.Modelos;
using MoedaEstudantil.Persistencia.Repositorios;
using MoedaEstudantil.Servicos;

namespace MoedaEstudantil.Controllers;

[Authorize]
[Route("alunos")]
public class AlunoController : ControladorBase
{
    private const string PerfilAluno = nameof(TipoUsuario.Aluno);

    private readonly AlunoServico _alunos;
    private readonly VantagemServico _vantagens;
    private readonly ResgateServico _resgates;
    private readonly NotificacaoServico _notificacoes;
    private readonly IInstituicaoRepositorio _instituicoes;

    public AlunoController(
        AlunoServico alunos,
        VantagemServico vantagens,
        ResgateServico resgates,
        NotificacaoServico notificacoes,
        IInstituicaoRepositorio instituicoes)
    {
        _alunos = alunos;
        _vantagens = vantagens;
        _resgates = resgates;
        _notificacoes = notificacoes;
        _instituicoes = instituicoes;
    }

    [AllowAnonymous]
    [HttpGet("novo")]
    public async Task<IActionResult> Novo(CancellationToken cancelamento)
    {
        var formulario = new AlunoFormulario();
        await PreencherInstituicoesAsync(formulario, cancelamento);
        return View("Formulario", formulario);
    }

    [AllowAnonymous]
    [HttpPost("novo")]
    public async Task<IActionResult> Novo(AlunoFormulario formulario, CancellationToken cancelamento)
    {
        if (!ModelState.IsValid)
        {
            await PreencherInstituicoesAsync(formulario, cancelamento);
            return View("Formulario", formulario);
        }

        var resultado = await _alunos.CadastrarAsync(ParaDados(formulario), cancelamento);
        if (!resultado.Sucesso)
            return await FormularioComErrosAsync(formulario, resultado, cancelamento);

        await IniciarSessaoAsync(resultado.Id, formulario);
        Informar("Cadastro realizado. O e-mail e a senha são o seu login.");
        return RedirectToAction(nameof(Detalhes), new { id = resultado.Id });
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Detalhes(long id, CancellationToken cancelamento)
    {
        if (!SessaoCookie.EhDono(User, id))
            return Forbid();

        var aluno = await _alunos.ObterAsync(id, cancelamento);
        return aluno is null ? PaginaNaoEncontrada() : View(aluno);
    }

    [HttpGet("{id:long}/editar")]
    public async Task<IActionResult> Editar(long id, CancellationToken cancelamento)
    {
        if (!SessaoCookie.EhDono(User, id))
            return Forbid();

        var aluno = await _alunos.ObterAsync(id, cancelamento);
        if (aluno is null)
            return PaginaNaoEncontrada();

        var formulario = ParaFormulario(aluno);
        await PreencherInstituicoesAsync(formulario, cancelamento);
        return View("Formulario", formulario);
    }

    [HttpPost("{id:long}/editar")]
    public async Task<IActionResult> Editar(long id, AlunoFormulario formulario, CancellationToken cancelamento)
    {
        if (!SessaoCookie.EhDono(User, id))
            return Forbid();

        formulario.Id = id;
        if (!ModelState.IsValid)
        {
            await PreencherInstituicoesAsync(formulario, cancelamento);
            return View("Formulario", formulario);
        }

        var resultado = await _alunos.AtualizarAsync(id, ParaDados(formulario), cancelamento);
        if (!resultado.Sucesso)
            return await FormularioComErrosAsync(formulario, resultado, cancelamento);

        await IniciarSessaoAsync(id, formulario);
        Informar("Cadastro atualizado.");
        return RedirectToAction(nameof(Detalhes), new { id });
    }

    [HttpGet("{id:long}/excluir")]
    public async Task<IActionResult> Excluir(long id, CancellationToken cancelamento)
    {
        if (!SessaoCookie.EhDono(User, id))
            return Forbid();

        var aluno = await _alunos.ObterAsync(id, cancelamento);
        return aluno is null ? PaginaNaoEncontrada() : View(aluno);
    }

    [HttpPost("{id:long}/excluir")]
    public async Task<IActionResult> ConfirmarExclusao(long id, CancellationToken cancelamento)
    {
        if (!SessaoCookie.EhDono(User, id))
            return Forbid();

        var resultado = await _alunos.ExcluirAsync(id, cancelamento);
        if (resultado.NaoEncontrado)
            return PaginaNaoEncontrada();
        if (!resultado.Sucesso)
        {
            Alertar(resultado.PrimeiroErro);
            return RedirectToAction(nameof(Detalhes), new { id });
        }

        await SessaoCookie.EncerrarAsync(HttpContext);
        Informar("Cadastro de aluno excluído.");
        return RedirectToAction("Index", "Home");
    }

    [Authorize(Roles = PerfilAluno)]
    [HttpGet("extrato")]
    public async Task<IActionResult> Extrato(CancellationToken cancelamento)
    {
        var extrato = await _alunos.ExtratoAsync(UsuarioId, cancelamento);
        return extrato is null ? PaginaNaoEncontrada() : View(extrato);
    }

    [Authorize(Roles = PerfilAluno)]
    [HttpGet("vantagens")]
    public async Task<IActionResult> Vantagens(CancellationToken cancelamento)
    {
        var aluno = await _alunos.ObterAsync(UsuarioId, cancelamento);
        if (aluno is null)
            return PaginaNaoEncontrada();

        var vantagens = await _vantagens.ListarDisponiveisAsync(cancelamento);
        return View(new VitrineVantagens(vantagens, aluno.Saldo));
    }

    [Authorize(Roles = PerfilAluno)]
    [HttpGet("vantagens/{vantagemId:long}/resgatar")]
    public async Task<IActionResult> Resgatar(long vantagemId, CancellationToken cancelamento)
    {
        var aluno = await _alunos.ObterAsync(UsuarioId, cancelamento);
        var vantagem = await _vantagens.ObterAsync(vantagemId, cancelamento);
        if (aluno is null || vantagem is null || !vantagem.Ativa)
            return PaginaNaoEncontrada();

        return View(new ConfirmacaoResgate(vantagem, aluno.Saldo));
    }

    [Authorize(Roles = PerfilAluno)]
    [HttpPost("vantagens/{vantagemId:long}/resgatar")]
    public async Task<IActionResult> ConfirmarResgate(long vantagemId, CancellationToken cancelamento)
    {
        var resultado = await _resgates.ResgatarAsync(UsuarioId, vantagemId, cancelamento);
        if (resultado.NaoEncontrado)
            return PaginaNaoEncontrada();
        if (!resultado.Sucesso)
        {
            Alertar(resultado.Erro);
            return RedirectToAction(nameof(Resgatar), new { vantagemId });
        }

        Informar("Troca realizada. O cupom também foi enviado para o seu e-mail.", resultado.Aviso);
        return RedirectToAction(nameof(Cupom), new { codigo = resultado.Valor!.Codigo });
    }

    [Authorize(Roles = PerfilAluno)]
    [HttpGet("cupons/{codigo}")]
    public async Task<IActionResult> Cupom(string codigo, CancellationToken cancelamento)
    {
        var cupom = await _resgates.ObterCupomDoAlunoAsync(UsuarioId, codigo, cancelamento);
        return cupom is null ? PaginaNaoEncontrada() : View(cupom);
    }

    [Authorize(Roles = PerfilAluno)]
    [HttpGet("notificacoes")]
    public async Task<IActionResult> Notificacoes(CancellationToken cancelamento)
        => View("Notificacoes", new CaixaNotificacoes(
            UsuarioEmail,
            await _notificacoes.ListarAsync(UsuarioEmail, cancelamento)));

    private Task IniciarSessaoAsync(long id, AlunoFormulario formulario)
        => SessaoCookie.IniciarAsync(HttpContext, new Sessao(
            id,
            formulario.Nome.Trim(),
            Normalizacao.Email(formulario.Email),
            TipoUsuario.Aluno));

    private async Task PreencherInstituicoesAsync(AlunoFormulario formulario, CancellationToken cancelamento)
    {
        var instituicoes = await _instituicoes.ListarAsync(cancelamento);
        formulario.Instituicoes = instituicoes.Select(instituicao => new SelectListItem(
            $"{instituicao.Nome} — {instituicao.Cidade}",
            instituicao.Id.ToString()));
    }

    private async Task<IActionResult> FormularioComErrosAsync(
        AlunoFormulario formulario,
        ResultadoCadastro resultado,
        CancellationToken cancelamento)
    {
        if (resultado.NaoEncontrado)
            return PaginaNaoEncontrada();

        foreach (var erro in resultado.Erros)
            ModelState.AddModelError(erro.Key, erro.Value);

        await PreencherInstituicoesAsync(formulario, cancelamento);
        return View("Formulario", formulario);
    }

    private static DadosAluno ParaDados(AlunoFormulario formulario) => new(
        formulario.Nome,
        formulario.Email,
        string.IsNullOrWhiteSpace(formulario.Senha) ? null : formulario.Senha,
        formulario.Cpf,
        formulario.Rg,
        formulario.Endereco,
        formulario.InstituicaoId,
        formulario.Curso);

    private static AlunoFormulario ParaFormulario(Aluno aluno) => new()
    {
        Id = aluno.Id,
        Nome = aluno.Nome,
        Email = aluno.Email,
        Cpf = Documento.FormatarCpf(aluno.Cpf),
        Rg = aluno.Rg,
        Endereco = aluno.Endereco,
        InstituicaoId = aluno.InstituicaoId,
        Curso = aluno.Curso
    };
}
