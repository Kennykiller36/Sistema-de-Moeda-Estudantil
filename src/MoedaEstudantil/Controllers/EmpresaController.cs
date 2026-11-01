using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoedaEstudantil.Dominio;
using MoedaEstudantil.Modelos;
using MoedaEstudantil.Servicos;

namespace MoedaEstudantil.Controllers;

[Authorize]
[Route("empresas")]
public class EmpresaController : Controller
{
    private readonly EmpresaServico _empresas;

    public EmpresaController(EmpresaServico empresas) => _empresas = empresas;

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancelamento)
        => View(await _empresas.ListarAsync(cancelamento));

    [AllowAnonymous]
    [HttpGet("novo")]
    public IActionResult Novo()
        => View("Formulario", new EmpresaFormulario());

    [AllowAnonymous]
    [HttpPost("novo")]
    public async Task<IActionResult> Novo(EmpresaFormulario formulario, CancellationToken cancelamento)
    {
        if (!ModelState.IsValid)
            return View("Formulario", formulario);

        var resultado = await _empresas.CadastrarAsync(ParaDados(formulario), cancelamento);
        if (!resultado.Sucesso)
            return FormularioComErros(formulario, resultado);

        await SessaoCookie.IniciarAsync(HttpContext, new Sessao(
            resultado.Id,
            formulario.NomeFantasia.Trim(),
            Normalizacao.Email(formulario.Email),
            TipoUsuario.Empresa));
        TempData["Mensagem"] = "Empresa cadastrada. O e-mail e a senha são o login de acesso.";
        return RedirectToAction(nameof(Detalhes), new { id = resultado.Id });
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Detalhes(long id, CancellationToken cancelamento)
    {
        var empresa = await _empresas.ObterAsync(id, cancelamento);
        return empresa is null ? PaginaNaoEncontrada() : View(empresa);
    }

    [HttpGet("{id:long}/editar")]
    public async Task<IActionResult> Editar(long id, CancellationToken cancelamento)
    {
        if (!SessaoCookie.EhDono(User, id))
            return AcessoNegado(id);

        var empresa = await _empresas.ObterAsync(id, cancelamento);
        if (empresa is null)
            return PaginaNaoEncontrada();

        return View("Formulario", ParaFormulario(empresa));
    }

    [HttpPost("{id:long}/editar")]
    public async Task<IActionResult> Editar(long id, EmpresaFormulario formulario, CancellationToken cancelamento)
    {
        if (!SessaoCookie.EhDono(User, id))
            return AcessoNegado(id);

        formulario.Id = id;
        if (!ModelState.IsValid)
            return View("Formulario", formulario);

        var resultado = await _empresas.AtualizarAsync(id, ParaDados(formulario), cancelamento);
        if (!resultado.Sucesso)
            return FormularioComErros(formulario, resultado);

        await SessaoCookie.IniciarAsync(HttpContext, new Sessao(
            id,
            formulario.NomeFantasia.Trim(),
            Normalizacao.Email(formulario.Email),
            TipoUsuario.Empresa));
        TempData["Mensagem"] = "Cadastro da empresa atualizado.";
        return RedirectToAction(nameof(Detalhes), new { id });
    }

    [HttpGet("{id:long}/excluir")]
    public async Task<IActionResult> Excluir(long id, CancellationToken cancelamento)
    {
        if (!SessaoCookie.EhDono(User, id))
            return AcessoNegado(id);

        var empresa = await _empresas.ObterAsync(id, cancelamento);
        return empresa is null ? PaginaNaoEncontrada() : View(empresa);
    }

    [HttpPost("{id:long}/excluir")]
    public async Task<IActionResult> ConfirmarExclusao(long id, CancellationToken cancelamento)
    {
        if (!SessaoCookie.EhDono(User, id))
            return AcessoNegado(id);

        if (!await _empresas.ExcluirAsync(id, cancelamento))
            return PaginaNaoEncontrada();

        await SessaoCookie.EncerrarAsync(HttpContext);
        TempData["Mensagem"] = "Cadastro da empresa excluído.";
        return RedirectToAction("Index", "Home");
    }

    private IActionResult FormularioComErros(EmpresaFormulario formulario, ResultadoCadastro resultado)
    {
        if (resultado.NaoEncontrado)
            return PaginaNaoEncontrada();

        foreach (var erro in resultado.Erros)
            ModelState.AddModelError(erro.Key, erro.Value);

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

    private static DadosEmpresa ParaDados(EmpresaFormulario formulario) => new(
        formulario.NomeFantasia,
        formulario.Email,
        string.IsNullOrWhiteSpace(formulario.Senha) ? null : formulario.Senha,
        formulario.Cnpj);

    private static EmpresaFormulario ParaFormulario(EmpresaParceira empresa) => new()
    {
        Id = empresa.Id,
        NomeFantasia = empresa.NomeFantasia,
        Email = empresa.Email,
        Cnpj = Documento.FormatarCnpj(empresa.Cnpj)
    };
}
