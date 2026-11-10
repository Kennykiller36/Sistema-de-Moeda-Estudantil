using Microsoft.EntityFrameworkCore;
using MoedaEstudantil.Dominio;
using MoedaEstudantil.Persistencia;
using MoedaEstudantil.Persistencia.Repositorios;

namespace MoedaEstudantil.Servicos;

public class AlunoServico
{
    private readonly IAlunoRepositorio _alunos;
    private readonly IUsuarioRepositorio _usuarios;
    private readonly IInstituicaoRepositorio _instituicoes;
    private readonly ITransacaoRepositorio _transacoes;
    private readonly IUnidadeDeTrabalho _unidade;

    public AlunoServico(
        IAlunoRepositorio alunos,
        IUsuarioRepositorio usuarios,
        IInstituicaoRepositorio instituicoes,
        ITransacaoRepositorio transacoes,
        IUnidadeDeTrabalho unidade)
    {
        _alunos = alunos;
        _usuarios = usuarios;
        _instituicoes = instituicoes;
        _transacoes = transacoes;
        _unidade = unidade;
    }

    public Task<Aluno?> ObterAsync(long id, CancellationToken cancelamento = default)
        => _alunos.ObterComInstituicaoAsync(id, cancelamento);

    public async Task<Extrato?> ExtratoAsync(long alunoId, CancellationToken cancelamento = default)
    {
        var aluno = await _alunos.ObterComInstituicaoAsync(alunoId, cancelamento);
        if (aluno is null)
            return null;

        var transacoes = await _transacoes.ListarDoAlunoAsync(alunoId, cancelamento);
        var movimentos = transacoes.Select(transacao => transacao.Tipo == TipoTransacao.Envio
            ? new MovimentoExtrato(
                transacao.DataHora, "Recebimento", $"De {transacao.Professor?.Nome}", transacao.Motivo, transacao.Quantidade)
            : new MovimentoExtrato(
                transacao.DataHora, "Troca", transacao.Vantagem?.Descricao ?? "Vantagem",
                transacao.Vantagem?.Empresa.NomeFantasia, -transacao.Quantidade,
                transacao.Cupom?.Codigo, transacao.Cupom?.Utilizado ?? false));

        return new Extrato(aluno.Nome, aluno.Saldo, movimentos);
    }

    public async Task<ResultadoCadastro> CadastrarAsync(DadosAluno dados, CancellationToken cancelamento = default)
    {
        var erros = await ValidarAsync(dados, ignorarId: null, exigirSenha: true, cancelamento);
        if (erros.Count > 0)
            return ResultadoCadastro.Falha(erros);

        var aluno = new Aluno
        {
            Nome = dados.Nome.Trim(),
            Email = Normalizacao.Email(dados.Email),
            Senha = SenhaHasher.Gerar(dados.Senha!),
            Tipo = TipoUsuario.Aluno,
            Cpf = Documento.SomenteDigitos(dados.Cpf),
            Rg = dados.Rg.Trim(),
            Endereco = dados.Endereco.Trim(),
            Curso = dados.Curso.Trim(),
            InstituicaoId = dados.InstituicaoId,
            Saldo = 0
        };

        await _alunos.AdicionarAsync(aluno, cancelamento);
        return await SalvarAsync(aluno, cancelamento);
    }

    public async Task<ResultadoCadastro> AtualizarAsync(long id, DadosAluno dados, CancellationToken cancelamento = default)
    {
        var aluno = await _alunos.ObterParaEdicaoAsync(id, cancelamento);
        if (aluno is null)
            return ResultadoCadastro.Ausente();

        var erros = await ValidarAsync(dados, id, exigirSenha: false, cancelamento);
        if (erros.Count > 0)
            return ResultadoCadastro.Falha(erros);

        aluno.Nome = dados.Nome.Trim();
        aluno.Email = Normalizacao.Email(dados.Email);
        aluno.Cpf = Documento.SomenteDigitos(dados.Cpf);
        aluno.Rg = dados.Rg.Trim();
        aluno.Endereco = dados.Endereco.Trim();
        aluno.Curso = dados.Curso.Trim();
        aluno.InstituicaoId = dados.InstituicaoId;
        if (!string.IsNullOrWhiteSpace(dados.Senha))
            aluno.Senha = SenhaHasher.Gerar(dados.Senha);

        return await SalvarAsync(aluno, cancelamento);
    }

    public async Task<ResultadoCadastro> ExcluirAsync(long id, CancellationToken cancelamento = default)
    {
        var aluno = await _alunos.ObterParaEdicaoAsync(id, cancelamento);
        if (aluno is null)
            return ResultadoCadastro.Ausente();

        if (await _transacoes.AlunoPossuiMovimentacaoAsync(id, cancelamento))
            return ResultadoCadastro.Falha(
                "O cadastro tem moedas recebidas ou trocadas e não pode ser excluído: o extrato dos professores e os cupons das empresas dependem dele.");

        _alunos.Remover(aluno);
        await _unidade.SalvarAsync(cancelamento);
        return ResultadoCadastro.Ok(id);
    }

    private async Task<ResultadoCadastro> SalvarAsync(Aluno aluno, CancellationToken cancelamento)
    {
        try
        {
            await _unidade.SalvarAsync(cancelamento);
            return ResultadoCadastro.Ok(aluno.Id);
        }
        catch (DbUpdateConcurrencyException)
        {
            _unidade.DescartarAlteracoes();
            return ResultadoCadastro.Falha("O cadastro mudou enquanto você editava. Abra de novo e repita a alteração.");
        }
        catch (DbUpdateException)
        {
            _unidade.DescartarAlteracoes();
            return ResultadoCadastro.Falha("Não foi possível salvar. E-mail ou CPF já cadastrado.");
        }
    }

    private async Task<Dictionary<string, string>> ValidarAsync(
        DadosAluno dados,
        long? ignorarId,
        bool exigirSenha,
        CancellationToken cancelamento)
    {
        var erros = new Dictionary<string, string>();
        var nome = dados.Nome?.Trim() ?? string.Empty;
        var email = Normalizacao.Email(dados.Email);
        var cpf = Documento.SomenteDigitos(dados.Cpf);
        var rg = dados.Rg?.Trim() ?? string.Empty;
        var endereco = dados.Endereco?.Trim() ?? string.Empty;
        var curso = dados.Curso?.Trim() ?? string.Empty;

        if (nome.Length is < 3 or > 160)
            erros["Nome"] = "Informe o nome completo.";
        if (!EmailValido(email))
            erros["Email"] = "Informe um e-mail válido.";
        else if (await _usuarios.EmailEmUsoAsync(email, ignorarId, cancelamento))
            erros["Email"] = "Este e-mail já está cadastrado.";

        if (!Documento.CpfValido(cpf))
            erros["Cpf"] = "Informe um CPF válido.";
        else if (await _alunos.CpfEmUsoAsync(cpf, ignorarId, cancelamento))
            erros["Cpf"] = "Este CPF já está cadastrado.";

        if (rg.Length is < 4 or > 20)
            erros["Rg"] = "Informe o RG.";
        if (endereco.Length is < 5 or > 240)
            erros["Endereco"] = "Informe o endereço.";
        if (curso.Length is < 2 or > 120)
            erros["Curso"] = "Informe o curso.";
        if (!await _instituicoes.ExisteAsync(dados.InstituicaoId, cancelamento))
            erros["InstituicaoId"] = "Selecione uma instituição pré-cadastrada.";

        if (exigirSenha && string.IsNullOrWhiteSpace(dados.Senha))
            erros["Senha"] = "Informe uma senha.";
        else if (!string.IsNullOrWhiteSpace(dados.Senha) && dados.Senha.Length < 6)
            erros["Senha"] = "A senha deve ter pelo menos 6 caracteres.";

        return erros;
    }

    internal static bool EmailValido(string email)
        => email.Length is >= 5 and <= 180
            && email.Count(caractere => caractere == '@') == 1
            && email.Split('@') is [var local, var dominio]
            && local.Length > 0
            && dominio.Contains('.')
            && !dominio.StartsWith('.')
            && !dominio.EndsWith('.');
}
