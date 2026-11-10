using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MoedaEstudantil.Models;

namespace MoedaEstudantil.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();

    [HttpGet("/acesso-negado")]
    public IActionResult AcessoNegado()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
