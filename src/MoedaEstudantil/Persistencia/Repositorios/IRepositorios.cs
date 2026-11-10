using MoedaEstudantil.Dominio;

namespace MoedaEstudantil.Persistencia.Repositorios;

public interface IAlunoRepositorio : IRepositorio<Aluno>
{
    Task<IReadOnlyList<Aluno>> ListarDaInstituicaoAsync(long instituicaoId, CancellationToken cancelamento = default);
    Task<Aluno?> ObterComInstituicaoAsync(long id, CancellationToken cancelamento = default);
    Task<Aluno?> ObterParaEdicaoAsync(long id, CancellationToken cancelamento = default);
    Task<bool> CpfEmUsoAsync(string cpf, long? ignorarId, CancellationToken cancelamento = default);
}

public interface IProfessorRepositorio
{
    Task<Professor?> ObterAsync(long id, CancellationToken cancelamento = default);
    Task<Professor?> ObterParaEdicaoAsync(long id, CancellationToken cancelamento = default);
    Task<IReadOnlyList<Professor>> ListarSemCreditoAsync(string semestre, CancellationToken cancelamento = default);
    Task<bool> PossuiCreditoAsync(long professorId, string semestre, CancellationToken cancelamento = default);
    Task<IReadOnlyList<CreditoSemestral>> ListarCreditosAsync(long professorId, CancellationToken cancelamento = default);
    Task AdicionarCreditoAsync(CreditoSemestral credito, CancellationToken cancelamento = default);
}

public interface IEmpresaParceiraRepositorio : IRepositorio<EmpresaParceira>
{
    Task<IReadOnlyList<EmpresaParceira>> ListarAsync(CancellationToken cancelamento = default);
    Task<EmpresaParceira?> ObterAsync(long id, CancellationToken cancelamento = default);
    Task<EmpresaParceira?> ObterParaEdicaoAsync(long id, CancellationToken cancelamento = default);
    Task<EmpresaParceira?> ObterComVantagensAsync(long id, CancellationToken cancelamento = default);
    Task<bool> CnpjEmUsoAsync(string cnpj, long? ignorarId, CancellationToken cancelamento = default);
}

public interface IVantagemRepositorio : IRepositorio<Vantagem>
{
    Task<IReadOnlyList<Vantagem>> ListarAtivasAsync(CancellationToken cancelamento = default);
    Task<IReadOnlyList<Vantagem>> ListarDaEmpresaAsync(long empresaId, CancellationToken cancelamento = default);
    Task<Vantagem?> ObterAsync(long id, CancellationToken cancelamento = default);
    Task<Vantagem?> ObterParaEdicaoAsync(long id, CancellationToken cancelamento = default);
    Task<bool> PossuiResgatesAsync(long id, CancellationToken cancelamento = default);
}

public interface ITransacaoRepositorio : IRepositorio<Transacao>
{
    Task<IReadOnlyList<Transacao>> ListarDoAlunoAsync(long alunoId, CancellationToken cancelamento = default);
    Task<IReadOnlyList<Transacao>> ListarEnviosDoProfessorAsync(long professorId, CancellationToken cancelamento = default);
    Task<bool> AlunoPossuiMovimentacaoAsync(long alunoId, CancellationToken cancelamento = default);
}

public interface ICupomRepositorio : IRepositorio<Cupom>
{
    Task<bool> CodigoEmUsoAsync(string codigo, CancellationToken cancelamento = default);
    Task<Cupom?> ObterDoAlunoAsync(long alunoId, string codigo, CancellationToken cancelamento = default);
    Task<Cupom?> ObterDaEmpresaAsync(long empresaId, string codigo, CancellationToken cancelamento = default);
    Task<IReadOnlyList<Cupom>> ListarDaEmpresaAsync(long empresaId, CancellationToken cancelamento = default);
    Task<bool> EmpresaPossuiCuponsAsync(long empresaId, CancellationToken cancelamento = default);
}

public interface INotificacaoRepositorio : IRepositorio<Notificacao>
{
    Task<IReadOnlyList<Notificacao>> ListarDoDestinatarioAsync(string email, CancellationToken cancelamento = default);
}

public interface IInstituicaoRepositorio
{
    Task<IReadOnlyList<Instituicao>> ListarAsync(CancellationToken cancelamento = default);
    Task<bool> ExisteAsync(long id, CancellationToken cancelamento = default);
}

public interface IUsuarioRepositorio
{
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancelamento = default);
    Task<bool> SessaoValidaAsync(long id, string email, TipoUsuario tipo, CancellationToken cancelamento = default);
    Task<bool> EmailEmUsoAsync(string email, long? ignorarId, CancellationToken cancelamento = default);
}
