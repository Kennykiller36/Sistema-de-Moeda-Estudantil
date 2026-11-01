using MoedaEstudantil.Dominio;

namespace MoedaEstudantil.Persistencia.Repositorios;

public interface IAlunoRepositorio : IRepositorio<Aluno>
{
    Task<IReadOnlyList<Aluno>> ListarComInstituicaoAsync(CancellationToken cancelamento = default);
    Task<Aluno?> ObterComInstituicaoAsync(long id, CancellationToken cancelamento = default);
    Task<Aluno?> ObterParaEdicaoAsync(long id, CancellationToken cancelamento = default);
    Task<bool> CpfEmUsoAsync(string cpf, long? ignorarId, CancellationToken cancelamento = default);
}

public interface IEmpresaParceiraRepositorio : IRepositorio<EmpresaParceira>
{
    Task<IReadOnlyList<EmpresaParceira>> ListarAsync(CancellationToken cancelamento = default);
    Task<EmpresaParceira?> ObterAsync(long id, CancellationToken cancelamento = default);
    Task<EmpresaParceira?> ObterParaEdicaoAsync(long id, CancellationToken cancelamento = default);
    Task<bool> CnpjEmUsoAsync(string cnpj, long? ignorarId, CancellationToken cancelamento = default);
}

public interface IInstituicaoRepositorio
{
    Task<IReadOnlyList<Instituicao>> ListarAsync(CancellationToken cancelamento = default);
    Task<bool> ExisteAsync(long id, CancellationToken cancelamento = default);
}

public interface IUsuarioRepositorio
{
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancelamento = default);
    Task<bool> EmailEmUsoAsync(string email, long? ignorarId, CancellationToken cancelamento = default);
}
