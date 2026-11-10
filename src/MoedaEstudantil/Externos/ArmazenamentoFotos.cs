namespace MoedaEstudantil.Externos;

public interface IArmazenamentoFotos
{
    Task<string> SalvarAsync(byte[] conteudo, string extensao, CancellationToken cancelamento = default);
    void Remover(string arquivo);
}

public class ArmazenamentoFotosEmDisco : IArmazenamentoFotos
{
    public const string RotaPublica = "/fotos";

    private readonly string _pasta;

    public static string Url(string arquivo) => $"{RotaPublica}/{Uri.EscapeDataString(arquivo)}";

    public ArmazenamentoFotosEmDisco(string pasta)
    {
        _pasta = pasta;
        Directory.CreateDirectory(pasta);
    }

    public async Task<string> SalvarAsync(byte[] conteudo, string extensao, CancellationToken cancelamento = default)
    {
        var arquivo = $"{Guid.NewGuid():N}{extensao}";
        await File.WriteAllBytesAsync(Path.Combine(_pasta, arquivo), conteudo, cancelamento);
        return arquivo;
    }

    public void Remover(string arquivo)
    {
        if (string.IsNullOrWhiteSpace(arquivo) || arquivo != Path.GetFileName(arquivo))
            return;

        var caminho = Path.Combine(_pasta, arquivo);
        if (File.Exists(caminho))
            File.Delete(caminho);
    }
}
