using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoedaEstudantil.Dominio;
using MoedaEstudantil.Modelos;
using MoedaEstudantil.Servicos;

namespace MoedaEstudantil.Controllers;

[Authorize]
[Route("empresas")]
public class EmpresaController : ControladorBase
{
    private const string PerfilEmpresa = nameof(TipoUsuario.Empresa);

    private readonly EmpresaServico _empresas;
    private readonly VantagemServico _vantagens;
    private readonly ResgateServico _resgates;
    private readonly NotificacaoServico _notificacoes;

    public EmpresaController(
        EmpresaServico empresas,
        VantagemServico vantagens,
        ResgateServico resgates,
        NotificacaoServico notificacoes)
    {
        _empresas = empresas;
        _vantagens = vantagens;
        _resgates = resgates;
        _notificacoes = notificacoes;
    }

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

        await IniciarSessaoAsync(resultado.Id, formulario);
        Informar("Empresa cadastrada. O e-mail e a senha são o login de acesso. Cadastre agora as vantagens oferecidas.");
        return RedirectToAction(nameof(Detalhes), new { id = resultado.Id });
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Detalhes(long id, CancellationToken cancelamento)
    {
        var empresa = await _empresas.ObterAsync(id, cancelamento);
        if (empresa is null)
            return PaginaNaoEncontrada();

        var vantagens = await _vantagens.ListarDaEmpresaAsync(id, cancelamento);
        if (!SessaoCookie.EhDono(User, id))
            vantagens = vantagens.Where(vantagem => vantagem.Ativa).ToList();
        return View(new DetalhesEmpresa(empresa, vantagens));
    }

    [HttpGet("{id:long}/editar")]
    public async Task<IActionResult> Editar(long id, CancellationToken cancelamento)
    {
        if (!SessaoCookie.EhDono(User, id))
            return Forbid();

        var empresa = await _empresas.ObterAsync(id, cancelamento);
        if (empresa is null)
            return PaginaNaoEncontrada();

        return View("Formulario", ParaFormulario(empresa));
    }

    [HttpPost("{id:long}/editar")]
    public async Task<IActionResult> Editar(long id, EmpresaFormulario formulario, CancellationToken cancelamento)
    {
        if (!SessaoCookie.EhDono(User, id))
            return Forbid();

        formulario.Id = id;
        if (!ModelState.IsValid)
            return View("Formulario", formulario);

        var resultado = await _empresas.AtualizarAsync(id, ParaDados(formulario), cancelamento);
        if (!resultado.Sucesso)
            return FormularioComErros(formulario, resultado);

        await IniciarSessaoAsync(id, formulario);
        Informar("Cadastro da empresa atualizado.");
        return RedirectToAction(nameof(Detalhes), new { id });
    }

    [HttpGet("{id:long}/excluir")]
    public async Task<IActionResult> Excluir(long id, CancellationToken cancelamento)
    {
        if (!SessaoCookie.EhDono(User, id))
            return Forbid();

        var empresa = await _empresas.ObterAsync(id, cancelamento);
        return empresa is null ? PaginaNaoEncontrada() : View(empresa);
    }

    [HttpPost("{id:long}/excluir")]
    public async Task<IActionResult> ConfirmarExclusao(long id, CancellationToken cancelamento)
    {
        if (!SessaoCookie.EhDono(User, id))
            return Forbid();

        var resultado = await _empresas.ExcluirAsync(id, cancelamento);
        if (resultado.NaoEncontrado)
            return PaginaNaoEncontrada();
        if (!resultado.Sucesso)
        {
            Alertar(resultado.PrimeiroErro);
            return RedirectToAction(nameof(Detalhes), new { id });
        }

        await SessaoCookie.EncerrarAsync(HttpContext);
        Informar("Cadastro da empresa excluído.");
        return RedirectToAction("Index", "Home");
    }

    [Authorize(Roles = PerfilEmpresa)]
    [HttpGet("vantagens")]
    public async Task<IActionResult> Vantagens(CancellationToken cancelamento)
        => View(await _vantagens.ListarDaEmpresaAsync(UsuarioId, cancelamento));

    [Authorize(Roles = PerfilEmpresa)]
    [HttpGet("vantagens/nova")]
    public IActionResult NovaVantagem() => View("FormularioVantagem", new VantagemFormulario());

    [Authorize(Roles = PerfilEmpresa)]
    [HttpPost("vantagens/nova")]
    public async Task<IActionResult> NovaVantagem(VantagemFormulario formulario, CancellationToken cancelamento)
    {
        if (!ModelState.IsValid)
            return View("FormularioVantagem", formulario);

        await using var foto = formulario.Foto?.OpenReadStream();
        var resultado = await _vantagens.CadastrarAsync(
            UsuarioId, ParaDados(formulario), ParaFoto(formulario.Foto, foto), cancelamento);
        if (!resultado.Sucesso)
            return FormularioVantagemComErros(formulario, resultado);

        Informar("Vantagem publicada na vitrine dos alunos.");
        return RedirectToAction(nameof(Vantagens));
    }

    [Authorize(Roles = PerfilEmpresa)]
    [HttpGet("vantagens/{vantagemId:long}/editar")]
    public async Task<IActionResult> EditarVantagem(long vantagemId, CancellationToken cancelamento)
    {
        var vantagem = await _vantagens.ObterAsync(vantagemId, cancelamento);
        if (vantagem is null || vantagem.EmpresaId != UsuarioId)
            return PaginaNaoEncontrada();

        return View("FormularioVantagem", new VantagemFormulario
        {
            Id = vantagem.Id,
            Descricao = vantagem.Descricao,
            CustoMoedas = vantagem.CustoMoedas,
            FotoAtual = vantagem.Foto
        });
    }

    [Authorize(Roles = PerfilEmpresa)]
    [HttpPost("vantagens/{vantagemId:long}/editar")]
    public async Task<IActionResult> EditarVantagem(
        long vantagemId,
        VantagemFormulario formulario,
        CancellationToken cancelamento)
    {
        formulario.Id = vantagemId;
        if (!ModelState.IsValid)
            return View("FormularioVantagem", formulario);

        await using var foto = formulario.Foto?.OpenReadStream();
        var resultado = await _vantagens.AtualizarAsync(
            UsuarioId, vantagemId, ParaDados(formulario), ParaFoto(formulario.Foto, foto), cancelamento);
        if (!resultado.Sucesso)
            return FormularioVantagemComErros(formulario, resultado);

        Informar("Vantagem atualizada.");
        return RedirectToAction(nameof(Vantagens));
    }

    [Authorize(Roles = PerfilEmpresa)]
    [HttpPost("vantagens/{vantagemId:long}/situacao")]
    public async Task<IActionResult> AlterarSituacao(long vantagemId, bool ativa, CancellationToken cancelamento)
    {
        var resultado = await _vantagens.AlterarSituacaoAsync(UsuarioId, vantagemId, ativa, cancelamento);
        if (resultado.NaoEncontrado)
            return PaginaNaoEncontrada();

        Informar(ativa ? "Vantagem de volta à vitrine." : "Vantagem retirada da vitrine.");
        return RedirectToAction(nameof(Vantagens));
    }

    [Authorize(Roles = PerfilEmpresa)]
    [HttpGet("vantagens/{vantagemId:long}/excluir")]
    public async Task<IActionResult> ExcluirVantagem(long vantagemId, CancellationToken cancelamento)
    {
        var vantagem = await _vantagens.ObterAsync(vantagemId, cancelamento);
        if (vantagem is null || vantagem.EmpresaId != UsuarioId)
            return PaginaNaoEncontrada();

        return View(vantagem);
    }

    [Authorize(Roles = PerfilEmpresa)]
    [HttpPost("vantagens/{vantagemId:long}/excluir")]
    public async Task<IActionResult> ConfirmarExclusaoVantagem(long vantagemId, CancellationToken cancelamento)
    {
        var resultado = await _vantagens.ExcluirAsync(UsuarioId, vantagemId, cancelamento);
        if (resultado.NaoEncontrado)
            return PaginaNaoEncontrada();

        if (resultado.Valor)
            Informar("Vantagem excluída.");
        else
            Informar("Vantagem desativada.", resultado.Aviso);
        return RedirectToAction(nameof(Vantagens));
    }

    [Authorize(Roles = PerfilEmpresa)]
    [HttpGet("cupons")]
    public async Task<IActionResult> Cupons(string? codigo, CancellationToken cancelamento)
    {
        var encontrado = string.IsNullOrWhiteSpace(codigo)
            ? null
            : await _resgates.LocalizarCupomAsync(UsuarioId, codigo, cancelamento);
        if (!string.IsNullOrWhiteSpace(codigo) && encontrado is null)
            ModelState.AddModelError(string.Empty, "Nenhum cupom da sua empresa tem esse código.");

        var cupons = await _resgates.ListarCuponsDaEmpresaAsync(UsuarioId, cancelamento);
        return View(new ConferenciaCupons(codigo?.Trim(), encontrado, cupons));
    }

    [Authorize(Roles = PerfilEmpresa)]
    [HttpPost("cupons/utilizar")]
    public async Task<IActionResult> UtilizarCupom(string? codigo, CancellationToken cancelamento)
    {
        var resultado = await _resgates.ConfirmarUtilizacaoAsync(UsuarioId, codigo, cancelamento);
        if (!resultado.Sucesso)
        {
            Alertar(resultado.Erro);
            return RedirectToAction(nameof(Cupons), new { codigo });
        }

        var cupom = resultado.Valor!;
        Informar($"Cupom {cupom.Codigo} confirmado: entregue \"{cupom.Vantagem.Descricao}\" para {cupom.Aluno.Nome}.");
        return RedirectToAction(nameof(Cupons));
    }

    [Authorize(Roles = PerfilEmpresa)]
    [HttpGet("notificacoes")]
    public async Task<IActionResult> Notificacoes(CancellationToken cancelamento)
        => View("Notificacoes", new CaixaNotificacoes(
            UsuarioEmail,
            await _notificacoes.ListarAsync(UsuarioEmail, cancelamento)));

    private Task IniciarSessaoAsync(long id, EmpresaFormulario formulario)
        => SessaoCookie.IniciarAsync(HttpContext, new Sessao(
            id,
            formulario.NomeFantasia.Trim(),
            Normalizacao.Email(formulario.Email),
            TipoUsuario.Empresa));

    private IActionResult FormularioComErros(EmpresaFormulario formulario, ResultadoCadastro resultado)
    {
        if (resultado.NaoEncontrado)
            return PaginaNaoEncontrada();

        foreach (var erro in resultado.Erros)
            ModelState.AddModelError(erro.Key, erro.Value);

        return View("Formulario", formulario);
    }

    private IActionResult FormularioVantagemComErros(VantagemFormulario formulario, ResultadoCadastro resultado)
    {
        if (resultado.NaoEncontrado)
            return PaginaNaoEncontrada();

        foreach (var erro in resultado.Erros)
            ModelState.AddModelError(erro.Key, erro.Value);

        return View("FormularioVantagem", formulario);
    }

    private static DadosEmpresa ParaDados(EmpresaFormulario formulario) => new(
        formulario.NomeFantasia,
        formulario.Email,
        string.IsNullOrWhiteSpace(formulario.Senha) ? null : formulario.Senha,
        formulario.Cnpj);

    private static DadosVantagem ParaDados(VantagemFormulario formulario)
        => new(formulario.Descricao, formulario.CustoMoedas!.Value);

    private static FotoEnviada? ParaFoto(IFormFile? arquivo, Stream? conteudo)
        => arquivo is null || conteudo is null ? null : new FotoEnviada(conteudo, arquivo.Length);

    private static EmpresaFormulario ParaFormulario(EmpresaParceira empresa) => new()
    {
        Id = empresa.Id,
        NomeFantasia = empresa.NomeFantasia,
        Email = empresa.Email,
        Cnpj = Documento.FormatarCnpj(empresa.Cnpj)
    };
}
