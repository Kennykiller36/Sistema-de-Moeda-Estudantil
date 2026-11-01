using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MoedaEstudantil.Servicos;

namespace MoedaEstudantil.Controllers;

public static class SessaoCookie
{
    public static async Task IniciarAsync(HttpContext http, Sessao sessao)
    {
        var identidade = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, sessao.Id.ToString()),
            new Claim(ClaimTypes.Name, sessao.Nome),
            new Claim(ClaimTypes.Email, sessao.Email),
            new Claim(ClaimTypes.Role, sessao.Tipo.ToString())
        ], CookieAuthenticationDefaults.AuthenticationScheme);

        await http.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidade));
    }

    public static Task EncerrarAsync(HttpContext http)
        => http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    public static long? Id(ClaimsPrincipal usuario)
        => long.TryParse(usuario.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public static bool EhDono(ClaimsPrincipal usuario, long id) => Id(usuario) == id;
}
