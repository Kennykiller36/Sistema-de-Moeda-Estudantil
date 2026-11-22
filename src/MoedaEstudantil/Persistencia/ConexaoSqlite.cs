using Microsoft.Data.Sqlite;

namespace MoedaEstudantil.Persistencia;

public static class ConexaoSqlite
{
    public static string Resolver(string conexao, string pastaRaiz)
    {
        var construtor = new SqliteConnectionStringBuilder(conexao);
        var origem = construtor.DataSource;
        if (origem is ":memory:")
            return construtor.ConnectionString;

        if (!Path.IsPathRooted(origem))
            origem = Path.GetFullPath(Path.Combine(pastaRaiz, origem));

        var pasta = Path.GetDirectoryName(origem);
        if (!string.IsNullOrEmpty(pasta))
            Directory.CreateDirectory(pasta);

        construtor.DataSource = origem;
        return construtor.ConnectionString;
    }
}
