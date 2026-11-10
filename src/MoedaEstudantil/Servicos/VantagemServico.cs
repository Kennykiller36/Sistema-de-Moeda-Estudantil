using MoedaEstudantil.Dominio;
using MoedaEstudantil.Externos;
using MoedaEstudantil.Persistencia;
using MoedaEstudantil.Persistencia.Repositorios;

namespace MoedaEstudantil.Servicos;

public class VantagemServico
{
    public const long TamanhoMaximoFoto = 5 * 1024 * 1024;
    public const int CustoMaximo = 100_000;

    private readonly IVantagemRepositorio _vantagens;
    private readonly IEmpresaParceiraRepositorio _empresas;
    private readonly IArmazenamentoFotos _fotos;
    private readonly IUnidadeDeTrabalho _unidade;

    public VantagemServico(
        IVantagemRepositorio vantagens,
        IEmpresaParceiraRepositorio empresas,
        IArmazenamentoFotos fotos,
        IUnidadeDeTrabalho unidade)
    {
        _vantagens = vantagens;
        _empresas = empresas;
        _fotos = fotos;
        _unidade = unidade;
    }

    public Task<IReadOnlyList<Vantagem>> ListarDisponiveisAsync(CancellationToken cancelamento = default)
        => _vantagens.ListarAtivasAsync(cancelamento);

    public Task<IReadOnlyList<Vantagem>> ListarDaEmpresaAsync(long empresaId, CancellationToken cancelamento = default)
        => _vantagens.ListarDaEmpresaAsync(empresaId, cancelamento);

    public Task<Vantagem?> ObterAsync(long id, CancellationToken cancelamento = default)
        => _vantagens.ObterAsync(id, cancelamento);

    public async Task<ResultadoCadastro> CadastrarAsync(
        long empresaId,
        DadosVantagem dados,
        FotoEnviada? foto,
        CancellationToken cancelamento = default)
    {
        var empresa = await _empresas.ObterParaEdicaoAsync(empresaId, cancelamento);
        if (empresa is null)
            return ResultadoCadastro.Ausente();

        var erros = Validar(dados);
        var imagem = await LerFotoAsync(foto, obrigatoria: true, erros, cancelamento);
        if (erros.Count > 0)
            return ResultadoCadastro.Falha(erros);

        var arquivo = await _fotos.SalvarAsync(imagem!.Conteudo, imagem.Extensao, cancelamento);
        var vantagem = new Vantagem
        {
            Descricao = dados.Descricao.Trim(),
            CustoMoedas = dados.CustoMoedas,
            Foto = arquivo
        };
        empresa.CadastrarVantagem(vantagem);
        await _vantagens.AdicionarAsync(vantagem, cancelamento);

        try
        {
            await _unidade.SalvarAsync(cancelamento);
        }
        catch
        {
            _fotos.Remover(arquivo);
            throw;
        }

        return ResultadoCadastro.Ok(vantagem.Id);
    }

    public async Task<ResultadoCadastro> AtualizarAsync(
        long empresaId,
        long vantagemId,
        DadosVantagem dados,
        FotoEnviada? foto,
        CancellationToken cancelamento = default)
    {
        var vantagem = await _vantagens.ObterParaEdicaoAsync(vantagemId, cancelamento);
        if (vantagem is null || vantagem.EmpresaId != empresaId)
            return ResultadoCadastro.Ausente();

        var erros = Validar(dados);
        var imagem = await LerFotoAsync(foto, obrigatoria: false, erros, cancelamento);
        if (erros.Count > 0)
            return ResultadoCadastro.Falha(erros);

        var fotoAnterior = vantagem.Foto;
        vantagem.Descricao = dados.Descricao.Trim();
        vantagem.CustoMoedas = dados.CustoMoedas;
        if (imagem is not null)
            vantagem.Foto = await _fotos.SalvarAsync(imagem.Conteudo, imagem.Extensao, cancelamento);

        try
        {
            await _unidade.SalvarAsync(cancelamento);
        }
        catch
        {
            if (imagem is not null)
                _fotos.Remover(vantagem.Foto);
            throw;
        }

        if (imagem is not null)
            _fotos.Remover(fotoAnterior);
        return ResultadoCadastro.Ok(vantagem.Id);
    }

    public async Task<Resultado<Vantagem>> AlterarSituacaoAsync(
        long empresaId,
        long vantagemId,
        bool ativa,
        CancellationToken cancelamento = default)
    {
        var vantagem = await _vantagens.ObterParaEdicaoAsync(vantagemId, cancelamento);
        if (vantagem is null || vantagem.EmpresaId != empresaId)
            return Resultado<Vantagem>.Ausente();

        if (ativa)
            vantagem.Publicar();
        else
            vantagem.Desativar();

        await _unidade.SalvarAsync(cancelamento);
        return Resultado<Vantagem>.Ok(vantagem);
    }

    public async Task<Resultado<bool>> ExcluirAsync(long empresaId, long vantagemId, CancellationToken cancelamento = default)
    {
        var vantagem = await _vantagens.ObterParaEdicaoAsync(vantagemId, cancelamento);
        if (vantagem is null || vantagem.EmpresaId != empresaId)
            return Resultado<bool>.Ausente();

        if (await _vantagens.PossuiResgatesAsync(vantagemId, cancelamento))
        {
            vantagem.Desativar();
            await _unidade.SalvarAsync(cancelamento);
            return Resultado<bool>.Ok(false,
                "Esta vantagem já foi resgatada e continua no histórico dos cupons. Ela foi desativada e saiu da vitrine.");
        }

        _vantagens.Remover(vantagem);
        await _unidade.SalvarAsync(cancelamento);
        _fotos.Remover(vantagem.Foto);
        return Resultado<bool>.Ok(true);
    }

    private static Dictionary<string, string> Validar(DadosVantagem dados)
    {
        var erros = new Dictionary<string, string>();
        var descricao = dados.Descricao?.Trim() ?? string.Empty;
        if (descricao.Length is < 5 or > 500)
            erros["Descricao"] = "Descreva a vantagem em 5 a 500 caracteres.";
        if (dados.CustoMoedas is < 1 or > CustoMaximo)
            erros["CustoMoedas"] = $"O custo deve ficar entre 1 e {CustoMaximo:N0} moedas.";
        return erros;
    }

    private static async Task<ImagemValidada?> LerFotoAsync(
        FotoEnviada? foto,
        bool obrigatoria,
        Dictionary<string, string> erros,
        CancellationToken cancelamento)
    {
        if (foto is null || foto.Tamanho == 0)
        {
            if (obrigatoria)
                erros["Foto"] = "Envie uma foto do produto.";
            return null;
        }

        if (foto.Tamanho > TamanhoMaximoFoto)
        {
            erros["Foto"] = "A foto deve ter no máximo 5 MB.";
            return null;
        }

        using var memoria = new MemoryStream();
        await foto.Conteudo.CopyToAsync(memoria, cancelamento);
        var conteudo = memoria.ToArray();
        var extensao = DetectarFormato(conteudo);
        if (extensao is null)
        {
            erros["Foto"] = "Use uma imagem JPG, PNG, WEBP ou GIF.";
            return null;
        }

        return new ImagemValidada(conteudo, extensao);
    }

    private static string? DetectarFormato(ReadOnlySpan<byte> bytes) => bytes switch
    {
        [0xFF, 0xD8, 0xFF, ..] => ".jpg",
        [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, ..] => ".png",
        [0x47, 0x49, 0x46, 0x38, ..] => ".gif",
        [0x52, 0x49, 0x46, 0x46, _, _, _, _, 0x57, 0x45, 0x42, 0x50, ..] => ".webp",
        _ => null
    };

    private sealed record ImagemValidada(byte[] Conteudo, string Extensao);
}
