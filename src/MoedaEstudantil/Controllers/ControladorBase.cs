using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace MoedaEstudantil.Controllers;

public abstract class ControladorBase : Controller
{
    protected long UsuarioId => SessaoCookie.Id(User)
        ?? throw new InvalidOperationException("A sessão não identifica o usuário.");

    protected string UsuarioEmail => User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    protected void Informar(string mensagem, string? aviso = null)
    {
        TempData["Mensagem"] = mensagem;
        if (aviso is not null)
            TempData["MensagemAviso"] = aviso;
    }

    protected void Alertar(string mensagem) => TempData["MensagemErro"] = mensagem;

    protected IActionResult PaginaNaoEncontrada()
    {
        Response.StatusCode = StatusCodes.Status404NotFound;
        return View("NaoEncontrado");
    }
}
