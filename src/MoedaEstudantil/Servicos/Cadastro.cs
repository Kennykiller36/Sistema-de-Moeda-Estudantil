namespace MoedaEstudantil.Servicos;

public static class Normalizacao
{
    public static string Email(string? email) => (email ?? string.Empty).Trim().ToLowerInvariant();
}

public static class Relogio
{
    public static DateTime Agora(this TimeProvider relogio) => relogio.GetLocalNow().DateTime;
}

public sealed record DadosAluno(
    string Nome,
    string Email,
    string? Senha,
    string Cpf,
    string Rg,
    string Endereco,
    long InstituicaoId,
    string Curso);

public sealed record DadosEmpresa(
    string NomeFantasia,
    string Email,
    string? Senha,
    string Cnpj);

public sealed record DadosVantagem(string Descricao, int CustoMoedas);

public sealed record FotoEnviada(Stream Conteudo, long Tamanho);

public sealed class ResultadoCadastro
{
    public bool Sucesso { get; private init; }
    public bool NaoEncontrado { get; private init; }
    public long Id { get; private init; }
    public IReadOnlyDictionary<string, string> Erros { get; private init; }
        = new Dictionary<string, string>();

    public string PrimeiroErro => Erros.Values.FirstOrDefault() ?? string.Empty;

    public static ResultadoCadastro Ok(long id) => new() { Sucesso = true, Id = id };

    public static ResultadoCadastro Falha(Dictionary<string, string> erros) => new() { Erros = erros };

    public static ResultadoCadastro Falha(string erro) => Falha(new Dictionary<string, string> { [string.Empty] = erro });

    public static ResultadoCadastro Ausente() => new() { NaoEncontrado = true };
}

public sealed class Resultado<T>
{
    public bool Sucesso { get; private init; }
    public bool NaoEncontrado { get; private init; }
    public T? Valor { get; private init; }
    public string Erro { get; private init; } = string.Empty;
    public string? Aviso { get; private init; }

    public static Resultado<T> Ok(T valor, string? aviso = null) => new() { Sucesso = true, Valor = valor, Aviso = aviso };

    public static Resultado<T> Falha(string erro) => new() { Erro = erro };

    public static Resultado<T> Ausente() => new() { NaoEncontrado = true, Erro = "Registro não encontrado." };
}
