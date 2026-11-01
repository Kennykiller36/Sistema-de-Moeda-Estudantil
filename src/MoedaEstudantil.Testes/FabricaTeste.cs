using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MoedaEstudantil.Dominio;
using MoedaEstudantil.Persistencia;
using MoedaEstudantil.Persistencia.Repositorios;
using MoedaEstudantil.Servicos;

namespace MoedaEstudantil.Testes;

public sealed class FabricaTeste : IDisposable
{
    private readonly SqliteConnection _conexao;

    public FabricaTeste()
    {
        _conexao = new SqliteConnection("Data Source=:memory:");
        _conexao.Open();
        var opcoes = new DbContextOptionsBuilder<MoedaEstudantilContext>()
            .UseSqlite(_conexao)
            .Options;
        Contexto = new MoedaEstudantilContext(opcoes);
        Contexto.Database.EnsureCreated();

        var instituicao = new Instituicao
        {
            Nome = "PUC Minas",
            Cnpj = "00000000000191",
            Cidade = "Belo Horizonte"
        };
        Contexto.Instituicoes.Add(instituicao);
        Contexto.SaveChanges();

        var alunos = new AlunoRepositorio(Contexto);
        var empresas = new EmpresaParceiraRepositorio(Contexto);
        var instituicoes = new InstituicaoRepositorio(Contexto);
        var usuarios = new UsuarioRepositorio(Contexto);
        Alunos = new AlunoServico(alunos, usuarios, instituicoes, Contexto);
        Empresas = new EmpresaServico(empresas, usuarios, Contexto);
        Autenticacao = new AutenticacaoServico(usuarios);
        InstituicaoId = instituicao.Id;
    }

    public MoedaEstudantilContext Contexto { get; }
    public AlunoServico Alunos { get; }
    public EmpresaServico Empresas { get; }
    public AutenticacaoServico Autenticacao { get; }
    public long InstituicaoId { get; }

    public DadosAluno AlunoExemplo(string email = "ana@pucminas.br", string cpf = "529.982.247-25") => new(
        "Ana Souza",
        email,
        "segredo",
        cpf,
        "MG1234567",
        "Rua das Flores, 10",
        InstituicaoId,
        "Engenharia de Software");

    public DadosEmpresa EmpresaExemplo(string email = "contato@cantina.br", string cnpj = "00.000.000/0001-91") => new(
        "Cantina Central",
        email,
        "segredo",
        cnpj);

    public void Dispose()
    {
        Contexto.Dispose();
        _conexao.Dispose();
    }
}
