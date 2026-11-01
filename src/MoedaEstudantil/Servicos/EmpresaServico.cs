using Microsoft.EntityFrameworkCore;
using MoedaEstudantil.Dominio;
using MoedaEstudantil.Persistencia;
using MoedaEstudantil.Persistencia.Repositorios;

namespace MoedaEstudantil.Servicos;

public class EmpresaServico
{
    private readonly IEmpresaParceiraRepositorio _empresas;
    private readonly IUsuarioRepositorio _usuarios;
    private readonly IUnidadeDeTrabalho _unidade;

    public EmpresaServico(
        IEmpresaParceiraRepositorio empresas,
        IUsuarioRepositorio usuarios,
        IUnidadeDeTrabalho unidade)
    {
        _empresas = empresas;
        _usuarios = usuarios;
        _unidade = unidade;
    }

    public Task<IReadOnlyList<EmpresaParceira>> ListarAsync(CancellationToken cancelamento = default)
        => _empresas.ListarAsync(cancelamento);

    public Task<EmpresaParceira?> ObterAsync(long id, CancellationToken cancelamento = default)
        => _empresas.ObterAsync(id, cancelamento);

    public async Task<ResultadoCadastro> CadastrarAsync(DadosEmpresa dados, CancellationToken cancelamento = default)
    {
        var erros = await ValidarAsync(dados, ignorarId: null, exigirSenha: true, cancelamento);
        if (erros.Count > 0)
            return ResultadoCadastro.Falha(erros);

        var empresa = new EmpresaParceira
        {
            NomeFantasia = dados.NomeFantasia.Trim(),
            Email = Normalizacao.Email(dados.Email),
            Senha = SenhaHasher.Gerar(dados.Senha!),
            Tipo = TipoUsuario.Empresa,
            Cnpj = Documento.SomenteDigitos(dados.Cnpj)
        };

        await _empresas.AdicionarAsync(empresa, cancelamento);
        return await SalvarAsync(empresa, cancelamento);
    }

    public async Task<ResultadoCadastro> AtualizarAsync(long id, DadosEmpresa dados, CancellationToken cancelamento = default)
    {
        var empresa = await _empresas.ObterParaEdicaoAsync(id, cancelamento);
        if (empresa is null)
            return ResultadoCadastro.Ausente();

        var erros = await ValidarAsync(dados, id, exigirSenha: false, cancelamento);
        if (erros.Count > 0)
            return ResultadoCadastro.Falha(erros);

        empresa.NomeFantasia = dados.NomeFantasia.Trim();
        empresa.Email = Normalizacao.Email(dados.Email);
        empresa.Cnpj = Documento.SomenteDigitos(dados.Cnpj);
        if (!string.IsNullOrWhiteSpace(dados.Senha))
            empresa.Senha = SenhaHasher.Gerar(dados.Senha);

        return await SalvarAsync(empresa, cancelamento);
    }

    public async Task<bool> ExcluirAsync(long id, CancellationToken cancelamento = default)
    {
        var empresa = await _empresas.ObterParaEdicaoAsync(id, cancelamento);
        if (empresa is null)
            return false;

        _empresas.Remover(empresa);
        await _unidade.SalvarAsync(cancelamento);
        return true;
    }

    private async Task<ResultadoCadastro> SalvarAsync(EmpresaParceira empresa, CancellationToken cancelamento)
    {
        try
        {
            await _unidade.SalvarAsync(cancelamento);
            return ResultadoCadastro.Ok(empresa.Id);
        }
        catch (DbUpdateException)
        {
            return ResultadoCadastro.Falha(new Dictionary<string, string>
            {
                [string.Empty] = "Não foi possível salvar. E-mail ou CNPJ já cadastrado."
            });
        }
    }

    private async Task<Dictionary<string, string>> ValidarAsync(
        DadosEmpresa dados,
        long? ignorarId,
        bool exigirSenha,
        CancellationToken cancelamento)
    {
        var erros = new Dictionary<string, string>();
        var nome = dados.NomeFantasia?.Trim() ?? string.Empty;
        var email = Normalizacao.Email(dados.Email);
        var cnpj = Documento.SomenteDigitos(dados.Cnpj);

        if (nome.Length is < 2 or > 160)
            erros["NomeFantasia"] = "Informe o nome fantasia.";
        if (!AlunoServico.EmailValido(email))
            erros["Email"] = "Informe um e-mail válido.";
        else if (await _usuarios.EmailEmUsoAsync(email, ignorarId, cancelamento))
            erros["Email"] = "Este e-mail já está cadastrado.";

        if (!Documento.CnpjValido(cnpj))
            erros["Cnpj"] = "Informe um CNPJ válido.";
        else if (await _empresas.CnpjEmUsoAsync(cnpj, ignorarId, cancelamento))
            erros["Cnpj"] = "Este CNPJ já está cadastrado.";

        if (exigirSenha && string.IsNullOrWhiteSpace(dados.Senha))
            erros["Senha"] = "Informe uma senha.";
        else if (!string.IsNullOrWhiteSpace(dados.Senha) && dados.Senha.Length < 6)
            erros["Senha"] = "A senha deve ter pelo menos 6 caracteres.";

        return erros;
    }
}
