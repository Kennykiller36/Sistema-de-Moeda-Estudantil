namespace MoedaEstudantil.Servicos;

public static class Normalizacao
{
    public static string Email(string? email) => (email ?? string.Empty).Trim().ToLowerInvariant();
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

public sealed class ResultadoCadastro
{
    public bool Sucesso { get; private init; }
    public bool NaoEncontrado { get; private init; }
    public long Id { get; private init; }
    public IReadOnlyDictionary<string, string> Erros { get; private init; }
        = new Dictionary<string, string>();

    public static ResultadoCadastro Ok(long id) => new() { Sucesso = true, Id = id };

    public static ResultadoCadastro Falha(Dictionary<string, string> erros) => new() { Erros = erros };

    public static ResultadoCadastro Ausente() => new() { NaoEncontrado = true };
}
