using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoedaEstudantil.Dominio;

namespace MoedaEstudantil.Persistencia;

public class UsuarioConfiguracao : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuario");
        builder.UseTptMappingStrategy();
        builder.Property(u => u.Id).HasColumnName("id");
        builder.Property(u => u.Email).HasColumnName("email").HasMaxLength(180).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.Senha).HasColumnName("senha").HasMaxLength(200).IsRequired();
        builder.Property(u => u.Tipo)
            .HasColumnName("tipo")
            .HasMaxLength(20)
            .HasConversion(ConversorEnum.Texto<TipoUsuario>())
            .IsRequired();
    }
}

public class InstituicaoConfiguracao : IEntityTypeConfiguration<Instituicao>
{
    public void Configure(EntityTypeBuilder<Instituicao> builder)
    {
        builder.ToTable("instituicao");
        builder.Property(i => i.Id).HasColumnName("id");
        builder.Property(i => i.Nome).HasColumnName("nome").HasMaxLength(160).IsRequired();
        builder.Property(i => i.Cnpj).HasColumnName("cnpj").HasMaxLength(14).IsRequired();
        builder.HasIndex(i => i.Cnpj).IsUnique();
        builder.Property(i => i.Cidade).HasColumnName("cidade").HasMaxLength(80).IsRequired();
    }
}

public class AlunoConfiguracao : IEntityTypeConfiguration<Aluno>
{
    public void Configure(EntityTypeBuilder<Aluno> builder)
    {
        builder.ToTable("aluno", tabela => tabela.HasCheckConstraint("ck_aluno_saldo", "saldo >= 0"));
        builder.Property(a => a.Id).HasColumnName("usuario_id");
        builder.Property(a => a.Nome).HasColumnName("nome").HasMaxLength(160).IsRequired();
        builder.Property(a => a.Cpf).HasColumnName("cpf").HasMaxLength(11).IsRequired();
        builder.HasIndex(a => a.Cpf).IsUnique();
        builder.Property(a => a.Rg).HasColumnName("rg").HasMaxLength(20).IsRequired();
        builder.Property(a => a.Endereco).HasColumnName("endereco").HasMaxLength(240).IsRequired();
        builder.Property(a => a.Curso).HasColumnName("curso").HasMaxLength(120).IsRequired();
        builder.Property(a => a.Saldo).HasColumnName("saldo").HasDefaultValue(0).IsRequired();
        builder.Property(a => a.InstituicaoId).HasColumnName("instituicao_id");
        builder.HasOne(a => a.Instituicao)
            .WithMany(i => i.Alunos)
            .HasForeignKey(a => a.InstituicaoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProfessorConfiguracao : IEntityTypeConfiguration<Professor>
{
    public void Configure(EntityTypeBuilder<Professor> builder)
    {
        builder.ToTable("professor", tabela => tabela.HasCheckConstraint("ck_professor_saldo", "saldo >= 0"));
        builder.Property(p => p.Id).HasColumnName("usuario_id");
        builder.Property(p => p.Nome).HasColumnName("nome").HasMaxLength(160).IsRequired();
        builder.Property(p => p.Cpf).HasColumnName("cpf").HasMaxLength(11).IsRequired();
        builder.HasIndex(p => p.Cpf).IsUnique();
        builder.Property(p => p.Departamento).HasColumnName("departamento").HasMaxLength(120).IsRequired();
        builder.Property(p => p.Saldo).HasColumnName("saldo").HasDefaultValue(0).IsRequired();
        builder.Property(p => p.InstituicaoId).HasColumnName("instituicao_id");
        builder.HasOne(p => p.Instituicao)
            .WithMany(i => i.Professores)
            .HasForeignKey(p => p.InstituicaoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class EmpresaParceiraConfiguracao : IEntityTypeConfiguration<EmpresaParceira>
{
    public void Configure(EntityTypeBuilder<EmpresaParceira> builder)
    {
        builder.ToTable("empresa_parceira");
        builder.Property(e => e.Id).HasColumnName("usuario_id");
        builder.Property(e => e.NomeFantasia).HasColumnName("nome_fantasia").HasMaxLength(160).IsRequired();
        builder.Property(e => e.Cnpj).HasColumnName("cnpj").HasMaxLength(14).IsRequired();
        builder.HasIndex(e => e.Cnpj).IsUnique();
    }
}

public class VantagemConfiguracao : IEntityTypeConfiguration<Vantagem>
{
    public void Configure(EntityTypeBuilder<Vantagem> builder)
    {
        builder.ToTable("vantagem", tabela => tabela.HasCheckConstraint("ck_vantagem_custo", "custo_moedas > 0"));
        builder.Property(v => v.Id).HasColumnName("id");
        builder.Property(v => v.Descricao).HasColumnName("descricao").HasMaxLength(500).IsRequired();
        builder.Property(v => v.Foto).HasColumnName("foto").HasMaxLength(300).IsRequired();
        builder.Property(v => v.CustoMoedas).HasColumnName("custo_moedas").IsRequired();
        builder.Property(v => v.Ativa).HasColumnName("ativa").HasDefaultValue(true).IsRequired();
        builder.Property(v => v.EmpresaId).HasColumnName("empresa_id");
        builder.HasOne(v => v.Empresa)
            .WithMany(e => e.Vantagens)
            .HasForeignKey(v => v.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TransacaoConfiguracao : IEntityTypeConfiguration<Transacao>
{
    public void Configure(EntityTypeBuilder<Transacao> builder)
    {
        builder.ToTable("transacao", tabela => tabela.HasCheckConstraint("ck_transacao_quantidade", "quantidade > 0"));
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.AlunoId).HasColumnName("aluno_id");
        builder.Property(t => t.ProfessorId).HasColumnName("professor_id");
        builder.Property(t => t.VantagemId).HasColumnName("vantagem_id");
        builder.Property(t => t.Tipo)
            .HasColumnName("tipo")
            .HasMaxLength(20)
            .HasConversion(ConversorEnum.Texto<TipoTransacao>())
            .IsRequired();
        builder.Property(t => t.Quantidade).HasColumnName("quantidade").IsRequired();
        builder.Property(t => t.Motivo).HasColumnName("motivo").HasMaxLength(500);
        builder.Property(t => t.DataHora).HasColumnName("data_hora").IsRequired();
        builder.HasOne(t => t.Aluno)
            .WithMany(a => a.Transacoes)
            .HasForeignKey(t => t.AlunoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.Professor)
            .WithMany(p => p.Transacoes)
            .HasForeignKey(t => t.ProfessorId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.Vantagem)
            .WithMany(v => v.Transacoes)
            .HasForeignKey(t => t.VantagemId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class CupomConfiguracao : IEntityTypeConfiguration<Cupom>
{
    public void Configure(EntityTypeBuilder<Cupom> builder)
    {
        builder.ToTable("cupom");
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.Codigo).HasColumnName("codigo").HasMaxLength(40).IsRequired();
        builder.HasIndex(c => c.Codigo).IsUnique();
        builder.Property(c => c.TransacaoId).HasColumnName("transacao_id");
        builder.HasIndex(c => c.TransacaoId).IsUnique();
        builder.Property(c => c.AlunoId).HasColumnName("aluno_id");
        builder.Property(c => c.VantagemId).HasColumnName("vantagem_id");
        builder.Property(c => c.EmpresaId).HasColumnName("empresa_id");
        builder.Property(c => c.Utilizado).HasColumnName("utilizado").HasDefaultValue(false).IsRequired();
        builder.Property(c => c.DataUtilizacao).HasColumnName("data_utilizacao");
        builder.HasOne(c => c.Transacao)
            .WithOne(t => t.Cupom)
            .HasForeignKey<Cupom>(c => c.TransacaoId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.Aluno)
            .WithMany(a => a.Cupons)
            .HasForeignKey(c => c.AlunoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.Vantagem)
            .WithMany(v => v.Cupons)
            .HasForeignKey(c => c.VantagemId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.Empresa)
            .WithMany(e => e.Cupons)
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class NotificacaoConfiguracao : IEntityTypeConfiguration<Notificacao>
{
    public void Configure(EntityTypeBuilder<Notificacao> builder)
    {
        builder.ToTable("notificacao");
        builder.Property(n => n.Id).HasColumnName("id");
        builder.Property(n => n.TransacaoId).HasColumnName("transacao_id");
        builder.Property(n => n.Destinatario).HasColumnName("destinatario").HasMaxLength(180).IsRequired();
        builder.Property(n => n.Tipo)
            .HasColumnName("tipo")
            .HasMaxLength(20)
            .HasConversion(ConversorEnum.Texto<TipoNotificacao>())
            .IsRequired();
        builder.Property(n => n.DataEnvio).HasColumnName("data_envio").IsRequired();
        builder.HasOne(n => n.Transacao)
            .WithMany(t => t.Notificacoes)
            .HasForeignKey(n => n.TransacaoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CreditoSemestralConfiguracao : IEntityTypeConfiguration<CreditoSemestral>
{
    public void Configure(EntityTypeBuilder<CreditoSemestral> builder)
    {
        builder.ToTable("credito_semestral", tabela =>
            tabela.HasCheckConstraint("ck_credito_quantidade", "quantidade = 1000"));
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.ProfessorId).HasColumnName("professor_id");
        builder.Property(c => c.Semestre).HasColumnName("semestre").HasMaxLength(6).IsRequired();
        builder.Property(c => c.Quantidade).HasColumnName("quantidade").HasDefaultValue(1000).IsRequired();
        builder.HasIndex(c => new { c.ProfessorId, c.Semestre }).IsUnique();
        builder.HasOne(c => c.Professor)
            .WithMany(p => p.Creditos)
            .HasForeignKey(c => c.ProfessorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
