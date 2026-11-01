using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MoedaEstudantil.Dominio;

namespace MoedaEstudantil.Persistencia;

public class MoedaEstudantilContext : DbContext, IUnidadeDeTrabalho
{
    public MoedaEstudantilContext(DbContextOptions<MoedaEstudantilContext> opcoes) : base(opcoes)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Aluno> Alunos => Set<Aluno>();
    public DbSet<Professor> Professores => Set<Professor>();
    public DbSet<EmpresaParceira> Empresas => Set<EmpresaParceira>();
    public DbSet<Instituicao> Instituicoes => Set<Instituicao>();
    public DbSet<Vantagem> Vantagens => Set<Vantagem>();
    public DbSet<Transacao> Transacoes => Set<Transacao>();
    public DbSet<Cupom> Cupons => Set<Cupom>();
    public DbSet<Notificacao> Notificacoes => Set<Notificacao>();
    public DbSet<CreditoSemestral> CreditosSemestrais => Set<CreditoSemestral>();

    public Task SalvarAsync(CancellationToken cancelamento = default) => SaveChangesAsync(cancelamento);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MoedaEstudantilContext).Assembly);
        RenomearChaveHerdada<Aluno>(modelBuilder, "aluno");
        RenomearChaveHerdada<Professor>(modelBuilder, "professor");
        RenomearChaveHerdada<EmpresaParceira>(modelBuilder, "empresa_parceira");
    }

    private static void RenomearChaveHerdada<T>(ModelBuilder modelBuilder, string tabela) where T : Usuario
    {
        var propriedade = modelBuilder.Entity<T>().Property(entidade => entidade.Id).Metadata;
        propriedade.SetColumnName("usuario_id", StoreObjectIdentifier.Table(tabela));
    }
}
