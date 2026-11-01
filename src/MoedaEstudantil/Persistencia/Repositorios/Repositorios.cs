using Microsoft.EntityFrameworkCore;
using MoedaEstudantil.Dominio;

namespace MoedaEstudantil.Persistencia.Repositorios;

public class AlunoRepositorio : RepositorioBase<Aluno>, IAlunoRepositorio
{
    public AlunoRepositorio(MoedaEstudantilContext contexto) : base(contexto)
    {
    }

    public async Task<IReadOnlyList<Aluno>> ListarComInstituicaoAsync(CancellationToken cancelamento = default)
        => await Contexto.Alunos
            .AsNoTracking()
            .Include(aluno => aluno.Instituicao)
            .OrderBy(aluno => aluno.Nome)
            .ToListAsync(cancelamento);

    public Task<Aluno?> ObterComInstituicaoAsync(long id, CancellationToken cancelamento = default)
        => Contexto.Alunos
            .AsNoTracking()
            .Include(aluno => aluno.Instituicao)
            .FirstOrDefaultAsync(aluno => aluno.Id == id, cancelamento);

    public Task<Aluno?> ObterParaEdicaoAsync(long id, CancellationToken cancelamento = default)
        => Contexto.Alunos.FirstOrDefaultAsync(aluno => aluno.Id == id, cancelamento);

    public Task<bool> CpfEmUsoAsync(string cpf, long? ignorarId, CancellationToken cancelamento = default)
        => Contexto.Alunos.AnyAsync(
            aluno => aluno.Cpf == cpf && (ignorarId == null || aluno.Id != ignorarId),
            cancelamento);
}

public class EmpresaParceiraRepositorio : RepositorioBase<EmpresaParceira>, IEmpresaParceiraRepositorio
{
    public EmpresaParceiraRepositorio(MoedaEstudantilContext contexto) : base(contexto)
    {
    }

    public async Task<IReadOnlyList<EmpresaParceira>> ListarAsync(CancellationToken cancelamento = default)
        => await Contexto.Empresas
            .AsNoTracking()
            .OrderBy(empresa => empresa.NomeFantasia)
            .ToListAsync(cancelamento);

    public Task<EmpresaParceira?> ObterAsync(long id, CancellationToken cancelamento = default)
        => Contexto.Empresas.AsNoTracking().FirstOrDefaultAsync(empresa => empresa.Id == id, cancelamento);

    public Task<EmpresaParceira?> ObterParaEdicaoAsync(long id, CancellationToken cancelamento = default)
        => Contexto.Empresas.FirstOrDefaultAsync(empresa => empresa.Id == id, cancelamento);

    public Task<bool> CnpjEmUsoAsync(string cnpj, long? ignorarId, CancellationToken cancelamento = default)
        => Contexto.Empresas.AnyAsync(
            empresa => empresa.Cnpj == cnpj && (ignorarId == null || empresa.Id != ignorarId),
            cancelamento);
}

public class InstituicaoRepositorio : IInstituicaoRepositorio
{
    private readonly MoedaEstudantilContext _contexto;

    public InstituicaoRepositorio(MoedaEstudantilContext contexto) => _contexto = contexto;

    public async Task<IReadOnlyList<Instituicao>> ListarAsync(CancellationToken cancelamento = default)
        => await _contexto.Instituicoes
            .AsNoTracking()
            .OrderBy(instituicao => instituicao.Nome)
            .ToListAsync(cancelamento);

    public Task<bool> ExisteAsync(long id, CancellationToken cancelamento = default)
        => _contexto.Instituicoes.AnyAsync(instituicao => instituicao.Id == id, cancelamento);
}

public class UsuarioRepositorio : IUsuarioRepositorio
{
    private readonly MoedaEstudantilContext _contexto;

    public UsuarioRepositorio(MoedaEstudantilContext contexto) => _contexto = contexto;

    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancelamento = default)
        => _contexto.Usuarios.FirstOrDefaultAsync(usuario => usuario.Email == email, cancelamento);

    public Task<bool> EmailEmUsoAsync(string email, long? ignorarId, CancellationToken cancelamento = default)
        => _contexto.Usuarios.AnyAsync(
            usuario => usuario.Email == email && (ignorarId == null || usuario.Id != ignorarId),
            cancelamento);
}
