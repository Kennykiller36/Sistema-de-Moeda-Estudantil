using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MoedaEstudantil.Dominio;
using MoedaEstudantil.Externos;
using MoedaEstudantil.Persistencia;
using MoedaEstudantil.Persistencia.Repositorios;
using MoedaEstudantil.Servicos;

namespace MoedaEstudantil.Testes;

public sealed class FabricaTeste : IDisposable
{
    public static readonly byte[] FotoPng = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 13];

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
        var outraInstituicao = new Instituicao
        {
            Nome = "UFMG",
            Cnpj = Documento.CompletarCnpj("200000000001"),
            Cidade = "Belo Horizonte"
        };
        Contexto.Instituicoes.AddRange(instituicao, outraInstituicao);
        Contexto.SaveChanges();

        var professor = new Professor
        {
            Nome = "Marina Couto",
            Email = "marina.couto@pucminas.br",
            Senha = SenhaHasher.Gerar("professor123"),
            Cpf = Documento.CompletarCpf("100000001"),
            Departamento = "Engenharia de Software",
            InstituicaoId = instituicao.Id
        };
        Contexto.Professores.Add(professor);
        Contexto.SaveChanges();

        var alunos = new AlunoRepositorio(Contexto);
        var professores = new ProfessorRepositorio(Contexto);
        var empresas = new EmpresaParceiraRepositorio(Contexto);
        var vantagens = new VantagemRepositorio(Contexto);
        var transacoes = new TransacaoRepositorio(Contexto);
        var cupons = new CupomRepositorio(Contexto);
        var notificacoes = new NotificacaoRepositorio(Contexto);
        var instituicoes = new InstituicaoRepositorio(Contexto);
        var usuarios = new UsuarioRepositorio(Contexto);

        Notificacoes = new NotificacaoServico(Email, notificacoes, Contexto, Relogio, NullLogger<NotificacaoServico>.Instance);
        Moedas = new MoedaServico(professores, alunos, transacoes, Contexto, Notificacoes, Relogio);
        Professores = new ProfessorServico(professores, alunos, transacoes, Contexto, Moedas);
        Alunos = new AlunoServico(alunos, usuarios, instituicoes, transacoes, Contexto);
        Empresas = new EmpresaServico(empresas, usuarios, vantagens, cupons, Fotos, Contexto);
        Vantagens = new VantagemServico(vantagens, empresas, Fotos, Contexto);
        Resgates = new ResgateServico(alunos, vantagens, transacoes, cupons, Contexto, Moedas, Notificacoes, Relogio);
        Autenticacao = new AutenticacaoServico(usuarios);
        InstituicaoId = instituicao.Id;
        OutraInstituicaoId = outraInstituicao.Id;
        ProfessorId = professor.Id;
    }

    public MoedaEstudantilContext Contexto { get; }
    public RelogioFixo Relogio { get; } = new(new DateTimeOffset(2026, 11, 9, 10, 0, 0, TimeSpan.Zero));
    public EmailFalso Email { get; } = new();
    public FotosEmMemoria Fotos { get; } = new();
    public AlunoServico Alunos { get; }
    public EmpresaServico Empresas { get; }
    public ProfessorServico Professores { get; }
    public MoedaServico Moedas { get; }
    public VantagemServico Vantagens { get; }
    public ResgateServico Resgates { get; }
    public NotificacaoServico Notificacoes { get; }
    public AutenticacaoServico Autenticacao { get; }
    public long InstituicaoId { get; }
    public long OutraInstituicaoId { get; }
    public long ProfessorId { get; }

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

    public static FotoEnviada Foto(byte[]? conteudo = null)
    {
        var bytes = conteudo ?? FotoPng;
        return new FotoEnviada(new MemoryStream(bytes), bytes.Length);
    }

    public void Dispose()
    {
        Contexto.Dispose();
        _conexao.Dispose();
    }
}

public sealed class RelogioFixo(DateTimeOffset agora) : TimeProvider
{
    public DateTimeOffset Agora { get; set; } = agora;
    public override DateTimeOffset GetUtcNow() => Agora;
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}

public sealed class EmailFalso : IServicoEmail
{
    public List<MensagemEmail> Enviados { get; } = [];
    public bool Falhar { get; set; }

    public Task EnviarAsync(MensagemEmail mensagem, CancellationToken cancelamento = default)
    {
        if (Falhar)
            throw new InvalidOperationException("Servidor SMTP indisponível.");

        Enviados.Add(mensagem);
        return Task.CompletedTask;
    }
}

public sealed class FotosEmMemoria : IArmazenamentoFotos
{
    public HashSet<string> Arquivos { get; } = [];

    public Task<string> SalvarAsync(byte[] conteudo, string extensao, CancellationToken cancelamento = default)
    {
        var arquivo = $"{Guid.NewGuid():N}{extensao}";
        Arquivos.Add(arquivo);
        return Task.FromResult(arquivo);
    }

    public void Remover(string arquivo) => Arquivos.Remove(arquivo);
}
