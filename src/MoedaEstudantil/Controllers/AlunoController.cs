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
public class AlunoController : Controller
{
    private readonly AlunoServico _alunos;
    private readonly IInstituicaoRepositorio _instituicoes;

    public AlunoController(AlunoServico alunos, IInstituicaoRepositorio instituicoes)
    {
        _alunos = alunos;
        _instituicoes = instituicoes;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancelamento)
        => View(await _alunos.ListarAsync(cancelamento));

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

        await SessaoCookie.IniciarAsync(HttpContext, new Sessao(
            resultado.Id,
            formulario.Nome.Trim(),
            Normalizacao.Email(formulario.Email),
            TipoUsuario.Aluno));
        TempData["Mensagem"] = "Cadastro realizado. O e-mail e a senha são o seu login.";
        return RedirectToAction(nameof(Detalhes), new { id = resultado.Id });
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Detalhes(long id, CancellationToken cancelamento)
    {
        var aluno = await _alunos.ObterAsync(id, cancelamento);
        return aluno is null ? PaginaNaoEncontrada() : View(aluno);
    }

    [HttpGet("{id:long}/editar")]
    public async Task<IActionResult> Editar(long id, CancellationToken cancelamento)
    {
        if (!SessaoCookie.EhDono(User, id))
            return AcessoNegado(id);

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
            return AcessoNegado(id);

        formulario.Id = id;
        if (!ModelState.IsValid)
        {
            await PreencherInstituicoesAsync(formulario, cancelamento);
            return View("Formulario", formulario);
        }

        var resultado = await _alunos.AtualizarAsync(id, ParaDados(formulario), cancelamento);
        if (!resultado.Sucesso)
            return await FormularioComErrosAsync(formulario, resultado, cancelamento);

        if (!string.IsNullOrWhiteSpace(formulario.Senha) || !string.Equals(User.Identity?.Name, formulario.Nome.Trim(), StringComparison.Ordinal))
        {
            await SessaoCookie.IniciarAsync(HttpContext, new Sessao(
                id,
                formulario.Nome.Trim(),
                Normalizacao.Email(formulario.Email),
                TipoUsuario.Aluno));
        }

        TempData["Mensagem"] = "Cadastro atualizado.";
        return RedirectToAction(nameof(Detalhes), new { id });
    }

    [HttpGet("{id:long}/excluir")]
    public async Task<IActionResult> Excluir(long id, CancellationToken cancelamento)
    {
        if (!SessaoCookie.EhDono(User, id))
            return AcessoNegado(id);

        var aluno = await _alunos.ObterAsync(id, cancelamento);
        return aluno is null ? PaginaNaoEncontrada() : View(aluno);
    }

    [HttpPost("{id:long}/excluir")]
    public async Task<IActionResult> ConfirmarExclusao(long id, CancellationToken cancelamento)
    {
        if (!SessaoCookie.EhDono(User, id))
            return AcessoNegado(id);

        if (!await _alunos.ExcluirAsync(id, cancelamento))
            return PaginaNaoEncontrada();

        await SessaoCookie.EncerrarAsync(HttpContext);
        TempData["Mensagem"] = "Cadastro de aluno excluído.";
        return RedirectToAction("Index", "Home");
    }

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

    private IActionResult AcessoNegado(long id)
    {
        TempData["MensagemErro"] = "Só é possível alterar o próprio cadastro.";
        return RedirectToAction(nameof(Detalhes), new { id });
    }

    private IActionResult PaginaNaoEncontrada()
    {
        Response.StatusCode = StatusCodes.Status404NotFound;
        return View("NaoEncontrado");
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
