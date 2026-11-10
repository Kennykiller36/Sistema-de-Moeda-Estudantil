using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MoedaEstudantil.Dominio;
using MoedaEstudantil.Modelos;
using MoedaEstudantil.Servicos;

namespace MoedaEstudantil.Controllers;

[Authorize(Roles = nameof(TipoUsuario.Professor))]
[Route("professor")]
public class ProfessorController : ControladorBase
{
    private readonly ProfessorServico _professores;
    private readonly MoedaServico _moedas;

    public ProfessorController(ProfessorServico professores, MoedaServico moedas)
    {
        _professores = professores;
        _moedas = moedas;
    }

    [HttpGet("")]
    public IActionResult Index() => RedirectToAction(nameof(Enviar));

    [HttpGet("enviar")]
    public async Task<IActionResult> Enviar(long? alunoId, CancellationToken cancelamento)
    {
        var formulario = new EnvioMoedasFormulario { AlunoId = alunoId ?? 0 };
        return await TelaDeEnvioAsync(formulario, cancelamento);
    }

    [HttpPost("enviar")]
    public async Task<IActionResult> Enviar(EnvioMoedasFormulario formulario, CancellationToken cancelamento)
    {
        if (!ModelState.IsValid)
            return await TelaDeEnvioAsync(formulario, cancelamento);

        var resultado = await _moedas.EnviarAsync(
            UsuarioId, formulario.AlunoId, formulario.Quantidade!.Value, formulario.Motivo, cancelamento);
        if (resultado.NaoEncontrado)
            return PaginaNaoEncontrada();
        if (!resultado.Sucesso)
        {
            ModelState.AddModelError(string.Empty, resultado.Erro);
            return await TelaDeEnvioAsync(formulario, cancelamento);
        }

        var envio = resultado.Valor!;
        Informar($"{envio.Quantidade} moedas enviadas para {envio.Aluno.Nome}. O aluno foi avisado por e-mail.", resultado.Aviso);
        return RedirectToAction(nameof(Extrato));
    }

    [HttpGet("extrato")]
    public async Task<IActionResult> Extrato(CancellationToken cancelamento)
    {
        var extrato = await _professores.ExtratoAsync(UsuarioId, cancelamento);
        return extrato is null ? PaginaNaoEncontrada() : View(extrato);
    }

    [HttpGet("alunos")]
    public async Task<IActionResult> Alunos(CancellationToken cancelamento)
    {
        var professor = await _professores.ObterAsync(UsuarioId, cancelamento);
        if (professor is null)
            return PaginaNaoEncontrada();

        ViewData["Instituicao"] = professor.Instituicao.Nome;
        return View(await _professores.ListarAlunosAsync(UsuarioId, cancelamento));
    }

    [HttpGet("senha")]
    public IActionResult Senha() => View(new SenhaFormulario());

    [HttpPost("senha")]
    public async Task<IActionResult> Senha(SenhaFormulario formulario, CancellationToken cancelamento)
    {
        if (!ModelState.IsValid)
            return View(formulario);

        var resultado = await _professores.AlterarSenhaAsync(
            UsuarioId, formulario.SenhaAtual, formulario.NovaSenha, cancelamento);
        if (resultado.NaoEncontrado)
            return PaginaNaoEncontrada();
        if (!resultado.Sucesso)
        {
            foreach (var erro in resultado.Erros)
                ModelState.AddModelError(erro.Key, erro.Value);
            return View(formulario);
        }

        Informar("Senha alterada.");
        return RedirectToAction(nameof(Enviar));
    }

    private async Task<IActionResult> TelaDeEnvioAsync(EnvioMoedasFormulario formulario, CancellationToken cancelamento)
    {
        var professor = await _professores.ObterAsync(UsuarioId, cancelamento);
        if (professor is null)
            return PaginaNaoEncontrada();

        var alunos = await _professores.ListarAlunosAsync(UsuarioId, cancelamento);
        formulario.Saldo = professor.Saldo;
        formulario.Instituicao = professor.Instituicao.Nome;
        formulario.Alunos = alunos.Select(aluno => new SelectListItem(
            $"{aluno.Nome} — {aluno.Curso}",
            aluno.Id.ToString(),
            aluno.Id == formulario.AlunoId));
        return View("Enviar", formulario);
    }
}
