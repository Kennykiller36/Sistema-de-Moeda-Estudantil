using Microsoft.Data.Sqlite;

namespace MoedaEstudantil.Persistencia;

public static class ConexaoSqlite
{
    public static string Resolver(string conexao, string pastaRaiz)
    {
        var construtor = new SqliteConnectionStringBuilder(conexao);
        if (!Path.IsPathRooted(construtor.DataSource))
        {
            var absoluto = Path.Combine(pastaRaiz, construtor.DataSource);
            var pasta = Path.GetDirectoryName(absoluto);
            if (!string.IsNullOrEmpty(pasta))
                Directory.CreateDirectory(pasta);
            construtor.DataSource = absoluto;
        }

        return construtor.ConnectionString;
    }
}
