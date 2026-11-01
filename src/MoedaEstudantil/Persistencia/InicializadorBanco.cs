using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoedaEstudantil.Dominio;

namespace MoedaEstudantil.Persistencia;

public static class InicializadorBanco
{
    private static readonly (string Nome, string BaseCnpj, string Cidade)[] Previstas =
    [
        ("Pontifícia Universidade Católica de Minas Gerais", "100000000001", "Belo Horizonte"),
        ("Universidade Federal de Minas Gerais", "200000000001", "Belo Horizonte"),
        ("Universidade Federal de Ouro Preto", "300000000001", "Ouro Preto")
    ];

    public static async Task AplicarAsync(IServiceProvider servicos)
    {
        using var escopo = servicos.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<MoedaEstudantilContext>();
        await contexto.Database.MigrateAsync();

        foreach (var prevista in Previstas)
        {
            var cnpj = Documento.CompletarCnpj(prevista.BaseCnpj);
            if (await contexto.Instituicoes.AnyAsync(instituicao => instituicao.Cnpj == cnpj))
                continue;

            contexto.Instituicoes.Add(new Instituicao
            {
                Nome = prevista.Nome,
                Cnpj = cnpj,
                Cidade = prevista.Cidade
            });
        }

        await contexto.SaveChangesAsync();
    }
}

public class MoedaEstudantilContextFactory : IDesignTimeDbContextFactory<MoedaEstudantilContext>
{
    public MoedaEstudantilContext CreateDbContext(string[] args)
    {
        var raiz = ProcurarRaiz();
        var configuracao = new ConfigurationBuilder()
            .SetBasePath(raiz)
            .AddJsonFile("appsettings.json")
            .Build();
        var conexao = ConexaoSqlite.Resolver(configuracao.GetConnectionString("Padrao")!, raiz);
        var opcoes = new DbContextOptionsBuilder<MoedaEstudantilContext>()
            .UseSqlite(conexao)
            .Options;
        return new MoedaEstudantilContext(opcoes);
    }

    private static string ProcurarRaiz()
    {
        var atual = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (atual is not null)
        {
            if (File.Exists(Path.Combine(atual.FullName, "appsettings.json"))
                && File.Exists(Path.Combine(atual.FullName, "MoedaEstudantil.csproj")))
                return atual.FullName;

            atual = atual.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
