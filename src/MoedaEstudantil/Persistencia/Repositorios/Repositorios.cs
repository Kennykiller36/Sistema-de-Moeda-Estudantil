using Microsoft.EntityFrameworkCore;
using MoedaEstudantil.Dominio;

namespace MoedaEstudantil.Persistencia.Repositorios;

public class AlunoRepositorio : RepositorioBase<Aluno>, IAlunoRepositorio
{
    public AlunoRepositorio(MoedaEstudantilContext contexto) : base(contexto)
    {
    }

    public async Task<IReadOnlyList<Aluno>> ListarDaInstituicaoAsync(long instituicaoId, CancellationToken cancelamento = default)
        => await Contexto.Alunos
            .AsNoTracking()
            .Include(aluno => aluno.Instituicao)
            .Where(aluno => aluno.InstituicaoId == instituicaoId)
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

public class ProfessorRepositorio : IProfessorRepositorio
{
    private readonly MoedaEstudantilContext _contexto;

    public ProfessorRepositorio(MoedaEstudantilContext contexto) => _contexto = contexto;

    public Task<Professor?> ObterAsync(long id, CancellationToken cancelamento = default)
        => _contexto.Professores
            .AsNoTracking()
            .Include(professor => professor.Instituicao)
            .FirstOrDefaultAsync(professor => professor.Id == id, cancelamento);

    public Task<Professor?> ObterParaEdicaoAsync(long id, CancellationToken cancelamento = default)
        => _contexto.Professores.FirstOrDefaultAsync(professor => professor.Id == id, cancelamento);

    public async Task<IReadOnlyList<Professor>> ListarSemCreditoAsync(string semestre, CancellationToken cancelamento = default)
        => await _contexto.Professores
            .Where(professor => !professor.Creditos.Any(credito => credito.Semestre == semestre))
            .ToListAsync(cancelamento);

    public Task<bool> PossuiCreditoAsync(long professorId, string semestre, CancellationToken cancelamento = default)
        => _contexto.CreditosSemestrais.AnyAsync(
            credito => credito.ProfessorId == professorId && credito.Semestre == semestre,
            cancelamento);

    public async Task<IReadOnlyList<CreditoSemestral>> ListarCreditosAsync(long professorId, CancellationToken cancelamento = default)
        => await _contexto.CreditosSemestrais
            .AsNoTracking()
            .Where(credito => credito.ProfessorId == professorId)
            .OrderByDescending(credito => credito.DataAplicacao)
            .ToListAsync(cancelamento);

    public Task AdicionarCreditoAsync(CreditoSemestral credito, CancellationToken cancelamento = default)
        => _contexto.CreditosSemestrais.AddAsync(credito, cancelamento).AsTask();
}

public class EmpresaParceiraRepositorio : RepositorioBase<EmpresaParceira>, IEmpresaParceiraRepositorio
{
    public EmpresaParceiraRepositorio(MoedaEstudantilContext contexto) : base(contexto)
    {
    }

    public async Task<IReadOnlyList<EmpresaParceira>> ListarAsync(CancellationToken cancelamento = default)
        => await Contexto.Empresas
            .AsNoTracking()
            .Include(empresa => empresa.Vantagens.Where(vantagem => vantagem.Ativa))
            .OrderBy(empresa => empresa.NomeFantasia)
            .ToListAsync(cancelamento);

    public Task<EmpresaParceira?> ObterAsync(long id, CancellationToken cancelamento = default)
        => Contexto.Empresas.AsNoTracking().FirstOrDefaultAsync(empresa => empresa.Id == id, cancelamento);

    public Task<EmpresaParceira?> ObterParaEdicaoAsync(long id, CancellationToken cancelamento = default)
        => Contexto.Empresas.FirstOrDefaultAsync(empresa => empresa.Id == id, cancelamento);

    public Task<EmpresaParceira?> ObterComVantagensAsync(long id, CancellationToken cancelamento = default)
        => Contexto.Empresas
            .Include(empresa => empresa.Vantagens)
            .FirstOrDefaultAsync(empresa => empresa.Id == id, cancelamento);

    public Task<bool> CnpjEmUsoAsync(string cnpj, long? ignorarId, CancellationToken cancelamento = default)
        => Contexto.Empresas.AnyAsync(
            empresa => empresa.Cnpj == cnpj && (ignorarId == null || empresa.Id != ignorarId),
            cancelamento);
}

public class VantagemRepositorio : RepositorioBase<Vantagem>, IVantagemRepositorio
{
    public VantagemRepositorio(MoedaEstudantilContext contexto) : base(contexto)
    {
    }

    public async Task<IReadOnlyList<Vantagem>> ListarAtivasAsync(CancellationToken cancelamento = default)
        => await Contexto.Vantagens
            .AsNoTracking()
            .Include(vantagem => vantagem.Empresa)
            .Where(vantagem => vantagem.Ativa)
            .OrderBy(vantagem => vantagem.CustoMoedas)
            .ThenBy(vantagem => vantagem.Descricao)
            .ToListAsync(cancelamento);

    public async Task<IReadOnlyList<Vantagem>> ListarDaEmpresaAsync(long empresaId, CancellationToken cancelamento = default)
        => await Contexto.Vantagens
            .AsNoTracking()
            .Include(vantagem => vantagem.Cupons)
            .Where(vantagem => vantagem.EmpresaId == empresaId)
            .OrderByDescending(vantagem => vantagem.Ativa)
            .ThenBy(vantagem => vantagem.Descricao)
            .ToListAsync(cancelamento);

    public Task<Vantagem?> ObterAsync(long id, CancellationToken cancelamento = default)
        => Contexto.Vantagens
            .AsNoTracking()
            .Include(vantagem => vantagem.Empresa)
            .FirstOrDefaultAsync(vantagem => vantagem.Id == id, cancelamento);

    public Task<Vantagem?> ObterParaEdicaoAsync(long id, CancellationToken cancelamento = default)
        => Contexto.Vantagens
            .Include(vantagem => vantagem.Empresa)
            .FirstOrDefaultAsync(vantagem => vantagem.Id == id, cancelamento);

    public Task<bool> PossuiResgatesAsync(long id, CancellationToken cancelamento = default)
        => Contexto.Transacoes.AnyAsync(transacao => transacao.VantagemId == id, cancelamento);
}

public class TransacaoRepositorio : RepositorioBase<Transacao>, ITransacaoRepositorio
{
    public TransacaoRepositorio(MoedaEstudantilContext contexto) : base(contexto)
    {
    }

    public async Task<IReadOnlyList<Transacao>> ListarDoAlunoAsync(long alunoId, CancellationToken cancelamento = default)
        => await Contexto.Transacoes
            .AsNoTracking()
            .Include(transacao => transacao.Professor)
            .Include(transacao => transacao.Vantagem!).ThenInclude(vantagem => vantagem.Empresa)
            .Include(transacao => transacao.Cupom)
            .Where(transacao => transacao.AlunoId == alunoId)
            .OrderByDescending(transacao => transacao.DataHora)
            .ToListAsync(cancelamento);

    public async Task<IReadOnlyList<Transacao>> ListarEnviosDoProfessorAsync(long professorId, CancellationToken cancelamento = default)
        => await Contexto.Transacoes
            .AsNoTracking()
            .Include(transacao => transacao.Aluno)
            .Where(transacao => transacao.ProfessorId == professorId && transacao.Tipo == TipoTransacao.Envio)
            .OrderByDescending(transacao => transacao.DataHora)
            .ToListAsync(cancelamento);

    public Task<bool> AlunoPossuiMovimentacaoAsync(long alunoId, CancellationToken cancelamento = default)
        => Contexto.Transacoes.AnyAsync(transacao => transacao.AlunoId == alunoId, cancelamento);
}

public class CupomRepositorio : RepositorioBase<Cupom>, ICupomRepositorio
{
    public CupomRepositorio(MoedaEstudantilContext contexto) : base(contexto)
    {
    }

    public Task<bool> CodigoEmUsoAsync(string codigo, CancellationToken cancelamento = default)
        => Contexto.Cupons.AnyAsync(cupom => cupom.Codigo == codigo, cancelamento);

    public Task<Cupom?> ObterDoAlunoAsync(long alunoId, string codigo, CancellationToken cancelamento = default)
        => Contexto.Cupons
            .AsNoTracking()
            .Include(cupom => cupom.Vantagem)
            .Include(cupom => cupom.Empresa)
            .Include(cupom => cupom.Transacao)
            .FirstOrDefaultAsync(cupom => cupom.AlunoId == alunoId && cupom.Codigo == codigo, cancelamento);

    public Task<Cupom?> ObterDaEmpresaAsync(long empresaId, string codigo, CancellationToken cancelamento = default)
        => Contexto.Cupons
            .Include(cupom => cupom.Aluno)
            .Include(cupom => cupom.Vantagem)
            .Include(cupom => cupom.Empresa)
            .FirstOrDefaultAsync(cupom => cupom.EmpresaId == empresaId && cupom.Codigo == codigo, cancelamento);

    public async Task<IReadOnlyList<Cupom>> ListarDaEmpresaAsync(long empresaId, CancellationToken cancelamento = default)
        => await Contexto.Cupons
            .AsNoTracking()
            .Include(cupom => cupom.Aluno)
            .Include(cupom => cupom.Vantagem)
            .Where(cupom => cupom.EmpresaId == empresaId)
            .OrderBy(cupom => cupom.Utilizado)
            .ThenByDescending(cupom => cupom.DataGeracao)
            .ToListAsync(cancelamento);

    public Task<bool> EmpresaPossuiCuponsAsync(long empresaId, CancellationToken cancelamento = default)
        => Contexto.Cupons.AnyAsync(cupom => cupom.EmpresaId == empresaId, cancelamento);
}

public class NotificacaoRepositorio : RepositorioBase<Notificacao>, INotificacaoRepositorio
{
    public NotificacaoRepositorio(MoedaEstudantilContext contexto) : base(contexto)
    {
    }

    public async Task<IReadOnlyList<Notificacao>> ListarDoDestinatarioAsync(string email, CancellationToken cancelamento = default)
        => await Contexto.Notificacoes
            .AsNoTracking()
            .Where(notificacao => notificacao.Destinatario == email)
            .OrderByDescending(notificacao => notificacao.DataEnvio)
            .ToListAsync(cancelamento);
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

    public Task<bool> SessaoValidaAsync(long id, string email, TipoUsuario tipo, CancellationToken cancelamento = default)
        => _contexto.Usuarios.AnyAsync(
            usuario => usuario.Id == id && usuario.Email == email && usuario.Tipo == tipo,
            cancelamento);

    public Task<bool> EmailEmUsoAsync(string email, long? ignorarId, CancellationToken cancelamento = default)
        => _contexto.Usuarios.AnyAsync(
            usuario => usuario.Email == email && (ignorarId == null || usuario.Id != ignorarId),
            cancelamento);
}
