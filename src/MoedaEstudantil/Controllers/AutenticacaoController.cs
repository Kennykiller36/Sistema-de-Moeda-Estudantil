using Microsoft.AspNetCore.Mvc;
using MoedaEstudantil.Modelos;
using MoedaEstudantil.Servicos;

namespace MoedaEstudantil.Controllers;

[Route("entrar")]
public class AutenticacaoController : Controller
{
    private readonly AutenticacaoServico _autenticacao;

    public AutenticacaoController(AutenticacaoServico autenticacao) => _autenticacao = autenticacao;

    [HttpGet("")]
    public IActionResult Entrar(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginFormulario());
    }

    [HttpPost("")]
    public async Task<IActionResult> Entrar(LoginFormulario formulario, string? returnUrl, CancellationToken cancelamento)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
            return View(formulario);

        var sessao = await _autenticacao.EntrarAsync(formulario.Email, formulario.Senha, cancelamento);
        if (sessao is null)
        {
            ModelState.AddModelError(string.Empty, "E-mail ou senha incorretos.");
            return View(formulario);
        }

        await SessaoCookie.IniciarAsync(HttpContext, sessao);
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost("/sair")]
    public async Task<IActionResult> Sair()
    {
        await SessaoCookie.EncerrarAsync(HttpContext);
        return RedirectToAction("Index", "Home");
    }
}
