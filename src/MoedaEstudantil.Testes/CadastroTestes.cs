using Microsoft.EntityFrameworkCore;
using MoedaEstudantil.Dominio;
using MoedaEstudantil.Servicos;

namespace MoedaEstudantil.Testes;

public class CadastroTestes
{
    [Fact]
    public void Esquema_segue_as_tabelas_e_a_chave_do_modelo_er()
    {
        using var fabrica = new FabricaTeste();
        var tabelas = LerColuna(fabrica, "SELECT name FROM sqlite_master WHERE type = 'table'");
        foreach (var tabela in new[]
        {
            "instituicao", "usuario", "aluno", "professor", "empresa_parceira",
            "vantagem", "transacao", "cupom", "notificacao", "credito_semestral"
        })
        {
            Assert.Contains(tabela, tabelas);
        }

        var colunasAluno = LerColuna(fabrica, "SELECT name FROM pragma_table_info('aluno')");
        Assert.Contains("usuario_id", colunasAluno);
        Assert.DoesNotContain("id", colunasAluno);
    }

    [Fact]
    public async Task Cadastrar_aluno_grava_login_saldo_zero_e_senha_protegida()
    {
        using var fabrica = new FabricaTeste();
        var resultado = await fabrica.Alunos.CadastrarAsync(fabrica.AlunoExemplo());

        Assert.True(resultado.Sucesso);
        var aluno = await fabrica.Alunos.ObterAsync(resultado.Id);
        Assert.NotNull(aluno);
        Assert.Equal("Ana Souza", aluno.Nome);
        Assert.Equal("ana@pucminas.br", aluno.Email);
        Assert.Equal("52998224725", aluno.Cpf);
        Assert.Equal(0, aluno.Saldo);
        Assert.Equal(TipoUsuario.Aluno, aluno.Tipo);
        Assert.NotEqual("segredo", aluno.Senha);
        Assert.Equal("ALUNO", LerColuna(fabrica, $"SELECT tipo FROM usuario WHERE id = {resultado.Id}").Single());

        var sessao = await fabrica.Autenticacao.EntrarAsync("Ana@pucminas.br", "segredo");
        Assert.NotNull(sessao);
        Assert.Equal(resultado.Id, sessao.Id);
        Assert.Null(await fabrica.Autenticacao.EntrarAsync("ana@pucminas.br", "errada"));
    }

    [Fact]
    public async Task Cadastrar_rejeita_cpf_invalido_instituicao_ausente_e_email_repetido()
    {
        using var fabrica = new FabricaTeste();
        var cpfInvalido = await fabrica.Alunos.CadastrarAsync(fabrica.AlunoExemplo(cpf: "111.111.111-11"));
        Assert.False(cpfInvalido.Sucesso);
        Assert.Equal("Informe um CPF válido.", cpfInvalido.Erros["Cpf"]);

        var semInstituicao = fabrica.AlunoExemplo(email: "outro@pucminas.br") with { InstituicaoId = 999 };
        var instituicao = await fabrica.Alunos.CadastrarAsync(semInstituicao);
        Assert.False(instituicao.Sucesso);
        Assert.Contains("InstituicaoId", instituicao.Erros.Keys);

        Assert.True((await fabrica.Alunos.CadastrarAsync(fabrica.AlunoExemplo())).Sucesso);
        var repetido = await fabrica.Empresas.CadastrarAsync(fabrica.EmpresaExemplo(email: "ana@pucminas.br"));
        Assert.False(repetido.Sucesso);
        Assert.Equal("Este e-mail já está cadastrado.", repetido.Erros["Email"]);
    }

    [Fact]
    public async Task Atualizar_aluno_mantem_senha_quando_o_campo_vem_vazio()
    {
        using var fabrica = new FabricaTeste();
        var criado = await fabrica.Alunos.CadastrarAsync(fabrica.AlunoExemplo());
        var atualizado = await fabrica.Alunos.AtualizarAsync(criado.Id, fabrica.AlunoExemplo() with
        {
            Senha = null,
            Curso = "Sistemas de Informação"
        });

        Assert.True(atualizado.Sucesso);
        var aluno = await fabrica.Alunos.ObterAsync(criado.Id);
        Assert.Equal("Sistemas de Informação", aluno!.Curso);
        Assert.NotNull(await fabrica.Autenticacao.EntrarAsync("ana@pucminas.br", "segredo"));
    }

    [Fact]
    public async Task Excluir_aluno_remove_o_cadastro_e_libera_o_email()
    {
        using var fabrica = new FabricaTeste();
        var criado = await fabrica.Alunos.CadastrarAsync(fabrica.AlunoExemplo());
        Assert.True((await fabrica.Alunos.ExcluirAsync(criado.Id)).Sucesso);
        Assert.Null(await fabrica.Alunos.ObterAsync(criado.Id));

        var empresa = await fabrica.Empresas.CadastrarAsync(fabrica.EmpresaExemplo(email: "ana@pucminas.br"));
        Assert.True(empresa.Sucesso);
    }

    [Fact]
    public async Task Empresa_faz_crud_e_recusa_cnpj_duplicado()
    {
        using var fabrica = new FabricaTeste();
        var criada = await fabrica.Empresas.CadastrarAsync(fabrica.EmpresaExemplo());
        Assert.True(criada.Sucesso);

        var duplicada = await fabrica.Empresas.CadastrarAsync(
            fabrica.EmpresaExemplo(email: "outra@cantina.br", cnpj: "00.000.000/0001-91"));
        Assert.False(duplicada.Sucesso);
        Assert.Equal("Este CNPJ já está cadastrado.", duplicada.Erros["Cnpj"]);

        var atualizada = await fabrica.Empresas.AtualizarAsync(criada.Id, fabrica.EmpresaExemplo() with
        {
            NomeFantasia = "Cantina do Bloco",
            Senha = "novasenha"
        });
        Assert.True(atualizada.Sucesso);
        Assert.Equal("Cantina do Bloco", (await fabrica.Empresas.ObterAsync(criada.Id))!.NomeFantasia);
        Assert.NotNull(await fabrica.Autenticacao.EntrarAsync("contato@cantina.br", "novasenha"));
        Assert.Null(await fabrica.Autenticacao.EntrarAsync("contato@cantina.br", "segredo"));

        Assert.True((await fabrica.Empresas.ExcluirAsync(criada.Id)).Sucesso);
        Assert.Empty(await fabrica.Empresas.ListarAsync());
    }

    private static List<string> LerColuna(FabricaTeste fabrica, string sql)
    {
        using var comando = fabrica.Contexto.Database.GetDbConnection().CreateCommand();
        comando.CommandText = sql;
        using var leitor = comando.ExecuteReader();
        var valores = new List<string>();
        while (leitor.Read())
            valores.Add(leitor.GetString(0));
        return valores;
    }
}
