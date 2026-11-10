using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MoedaEstudantil.Dominio;
using MoedaEstudantil.Persistencia;
using MoedaEstudantil.Servicos;

namespace MoedaEstudantil.Testes;

public class MoedaTestes
{
    [Fact]
    public async Task Professor_recebe_1000_moedas_uma_vez_por_semestre_e_o_saldo_acumula()
    {
        using var fabrica = new FabricaTeste();

        Assert.Equal(1, await fabrica.Moedas.CreditarSemestreDeTodosAsync());
        Assert.Equal(0, await fabrica.Moedas.CreditarSemestreDeTodosAsync());
        Assert.Equal(1000, (await fabrica.Professores.ObterAsync(fabrica.ProfessorId))!.Saldo);

        fabrica.Relogio.Agora = new DateTimeOffset(2027, 2, 10, 8, 0, 0, TimeSpan.Zero);
        var professor = await fabrica.Professores.ObterAsync(fabrica.ProfessorId);

        Assert.Equal(2000, professor!.Saldo);
        Assert.Equal("2027/1", fabrica.Moedas.SemestreAtual);
    }

    [Fact]
    public async Task Envio_desconta_do_professor_credita_o_aluno_e_avisa_por_email()
    {
        using var fabrica = new FabricaTeste();
        var aluno = await fabrica.Alunos.CadastrarAsync(fabrica.AlunoExemplo());

        var envio = await fabrica.Moedas.EnviarAsync(fabrica.ProfessorId, aluno.Id, 300, "Melhor projeto da turma");

        Assert.True(envio.Sucesso, envio.Erro);
        Assert.Null(envio.Aviso);
        Assert.Equal(700, (await fabrica.Professores.ObterAsync(fabrica.ProfessorId))!.Saldo);
        Assert.Equal(300, (await fabrica.Alunos.ObterAsync(aluno.Id))!.Saldo);
        var email = Assert.Single(fabrica.Email.Enviados);
        Assert.Equal("ana@pucminas.br", email.Para);
        Assert.Equal("Você recebeu 300 moedas", email.Assunto);
        Assert.Contains("Melhor projeto da turma", email.Texto);
        Assert.Single(await fabrica.Notificacoes.ListarAsync("ana@pucminas.br"));
    }

    [Fact]
    public async Task Envio_exige_motivo_saldo_suficiente_e_aluno_da_mesma_instituicao()
    {
        using var fabrica = new FabricaTeste();
        var aluno = await fabrica.Alunos.CadastrarAsync(fabrica.AlunoExemplo());
        var deFora = await fabrica.Alunos.CadastrarAsync(
            fabrica.AlunoExemplo("rui@ufmg.br", Documento.CompletarCpf("123456789")) with { InstituicaoId = fabrica.OutraInstituicaoId });

        var semMotivo = await fabrica.Moedas.EnviarAsync(fabrica.ProfessorId, aluno.Id, 10, "  ");
        var semSaldo = await fabrica.Moedas.EnviarAsync(fabrica.ProfessorId, aluno.Id, 1001, "Monitoria");
        var outraInstituicao = await fabrica.Moedas.EnviarAsync(fabrica.ProfessorId, deFora.Id, 10, "Monitoria");

        Assert.Equal("Informe o motivo do reconhecimento.", semMotivo.Erro);
        Assert.Equal("Saldo insuficiente: você tem 1000 moedas.", semSaldo.Erro);
        Assert.Equal("O aluno precisa ser da mesma instituição do professor.", outraInstituicao.Erro);
        Assert.Equal(1000, (await fabrica.Professores.ObterAsync(fabrica.ProfessorId))!.Saldo);
        Assert.Equal(0, (await fabrica.Alunos.ObterAsync(aluno.Id))!.Saldo);
        Assert.Empty(fabrica.Email.Enviados);
    }

    [Fact]
    public async Task Falha_no_email_nao_desfaz_o_envio()
    {
        using var fabrica = new FabricaTeste();
        var aluno = await fabrica.Alunos.CadastrarAsync(fabrica.AlunoExemplo());
        fabrica.Email.Falhar = true;

        var envio = await fabrica.Moedas.EnviarAsync(fabrica.ProfessorId, aluno.Id, 50, "Participação");

        Assert.True(envio.Sucesso);
        Assert.NotNull(envio.Aviso);
        Assert.Equal(50, (await fabrica.Alunos.ObterAsync(aluno.Id))!.Saldo);
        Assert.Empty(await fabrica.Notificacoes.ListarAsync("ana@pucminas.br"));
    }

    [Fact]
    public async Task Resgate_desconta_o_saldo_gera_cupom_unico_e_avisa_aluno_e_empresa()
    {
        using var fabrica = new FabricaTeste();
        var (alunoId, empresaId, vantagemId) = await PrepararTrocaAsync(fabrica);

        var resgate = await fabrica.Resgates.ResgatarAsync(alunoId, vantagemId);

        Assert.True(resgate.Sucesso, resgate.Erro);
        var cupom = resgate.Valor!;
        Assert.Matches("^ME-[A-HJ-NP-Z2-9]{4}-[A-HJ-NP-Z2-9]{4}$", cupom.Codigo);
        Assert.Equal(empresaId, cupom.EmpresaId);
        Assert.Equal(100, (await fabrica.Alunos.ObterAsync(alunoId))!.Saldo);

        var emails = fabrica.Email.Enviados.Where(email => email.Destaque == cupom.Codigo).ToList();
        Assert.Equal(["ana@pucminas.br", "contato@cantina.br"], emails.Select(email => email.Para));

        var semSaldo = await fabrica.Resgates.ResgatarAsync(alunoId, vantagemId);
        Assert.Equal("Saldo insuficiente: a vantagem custa 200 moedas e você tem 100.", semSaldo.Erro);
    }

    [Fact]
    public async Task Empresa_confirma_o_cupom_uma_unica_vez()
    {
        using var fabrica = new FabricaTeste();
        var (alunoId, empresaId, vantagemId) = await PrepararTrocaAsync(fabrica);
        var codigo = (await fabrica.Resgates.ResgatarAsync(alunoId, vantagemId)).Valor!.Codigo;
        var outraEmpresa = await fabrica.Empresas.CadastrarAsync(
            fabrica.EmpresaExemplo("vendas@livraria.br", "11.222.333/0001-81"));

        var deOutraEmpresa = await fabrica.Resgates.ConfirmarUtilizacaoAsync(outraEmpresa.Id, codigo);
        var primeira = await fabrica.Resgates.ConfirmarUtilizacaoAsync(empresaId, codigo.ToLowerInvariant());
        var segunda = await fabrica.Resgates.ConfirmarUtilizacaoAsync(empresaId, codigo);

        Assert.Equal("Nenhum cupom da sua empresa tem esse código.", deOutraEmpresa.Erro);
        Assert.True(primeira.Sucesso, primeira.Erro);
        Assert.StartsWith($"O cupom {codigo} já foi utilizado", segunda.Erro);
        Assert.True((await fabrica.Resgates.ObterCupomDoAlunoAsync(alunoId, codigo))!.Utilizado);
    }

    [Fact]
    public async Task Extratos_listam_os_movimentos_com_o_saldo_apos_cada_um()
    {
        using var fabrica = new FabricaTeste();
        var (alunoId, _, vantagemId) = await PrepararTrocaAsync(fabrica);
        fabrica.Relogio.Agora = fabrica.Relogio.Agora.AddHours(1);
        await fabrica.Resgates.ResgatarAsync(alunoId, vantagemId);

        var doAluno = (await fabrica.Alunos.ExtratoAsync(alunoId))!;
        var doProfessor = (await fabrica.Professores.ExtratoAsync(fabrica.ProfessorId))!;

        Assert.Equal(
            [("Troca", -200, 100), ("Recebimento", 300, 300)],
            doAluno.Linhas.Select(linha => (linha.Movimento.Tipo, linha.Movimento.Quantidade, linha.SaldoApos)));
        Assert.NotNull(doAluno.Linhas[0].Movimento.CodigoCupom);
        Assert.Equal(
            [("Envio", -300, 700), ("Crédito semestral", 1000, 1000)],
            doProfessor.Linhas.Select(linha => (linha.Movimento.Tipo, linha.Movimento.Quantidade, linha.SaldoApos)));
    }

    [Fact]
    public async Task Vantagem_exige_descricao_custo_positivo_e_foto_de_imagem()
    {
        using var fabrica = new FabricaTeste();
        var empresa = await fabrica.Empresas.CadastrarAsync(fabrica.EmpresaExemplo());

        var semFoto = await fabrica.Vantagens.CadastrarAsync(empresa.Id, new DadosVantagem("Café grátis", 50), null);
        var textoComoFoto = await fabrica.Vantagens.CadastrarAsync(
            empresa.Id, new DadosVantagem("Café grátis", 50), FabricaTeste.Foto("não é imagem"u8.ToArray()));
        var custoZero = await fabrica.Vantagens.CadastrarAsync(
            empresa.Id, new DadosVantagem("Café", 0), FabricaTeste.Foto());

        Assert.Equal("Envie uma foto do produto.", semFoto.Erros["Foto"]);
        Assert.Equal("Use uma imagem JPG, PNG, WEBP ou GIF.", textoComoFoto.Erros["Foto"]);
        Assert.True(custoZero.Erros.ContainsKey("Descricao"));
        Assert.True(custoZero.Erros.ContainsKey("CustoMoedas"));
        Assert.Empty(fabrica.Fotos.Arquivos);
    }

    [Fact]
    public async Task Exclusoes_preservam_o_historico_de_moedas_e_cupons()
    {
        using var fabrica = new FabricaTeste();
        var (alunoId, empresaId, vantagemId) = await PrepararTrocaAsync(fabrica);
        await fabrica.Resgates.ResgatarAsync(alunoId, vantagemId);

        var aluno = await fabrica.Alunos.ExcluirAsync(alunoId);
        var empresa = await fabrica.Empresas.ExcluirAsync(empresaId);
        var vantagem = await fabrica.Vantagens.ExcluirAsync(empresaId, vantagemId);

        Assert.False(aluno.Sucesso);
        Assert.False(empresa.Sucesso);
        Assert.True(vantagem.Sucesso);
        Assert.False(vantagem.Valor);
        Assert.Empty(await fabrica.Vantagens.ListarDisponiveisAsync());
        Assert.Single(fabrica.Fotos.Arquivos);
    }

    [Fact]
    public async Task Migracoes_criam_as_colunas_da_sprint_3()
    {
        using var conexao = new SqliteConnection("Data Source=:memory:");
        conexao.Open();
        var opcoes = new DbContextOptionsBuilder<MoedaEstudantilContext>().UseSqlite(conexao).Options;
        using var contexto = new MoedaEstudantilContext(opcoes);

        await contexto.Database.MigrateAsync();

        Assert.Empty(await contexto.Database.GetPendingMigrationsAsync());
        Assert.Contains("assunto", Colunas(conexao, "notificacao"));
        Assert.Contains("corpo", Colunas(conexao, "notificacao"));
        Assert.Contains("data_geracao", Colunas(conexao, "cupom"));
        Assert.Contains("data_aplicacao", Colunas(conexao, "credito_semestral"));
    }

    private static async Task<(long AlunoId, long EmpresaId, long VantagemId)> PrepararTrocaAsync(FabricaTeste fabrica)
    {
        var aluno = await fabrica.Alunos.CadastrarAsync(fabrica.AlunoExemplo());
        var empresa = await fabrica.Empresas.CadastrarAsync(fabrica.EmpresaExemplo());
        var vantagem = await fabrica.Vantagens.CadastrarAsync(
            empresa.Id, new DadosVantagem("Almoço executivo na cantina", 200), FabricaTeste.Foto());
        var envio = await fabrica.Moedas.EnviarAsync(fabrica.ProfessorId, aluno.Id, 300, "Melhor projeto da turma");
        Assert.True(envio.Sucesso && vantagem.Sucesso);
        return (aluno.Id, empresa.Id, vantagem.Id);
    }

    private static List<string> Colunas(SqliteConnection conexao, string tabela)
    {
        using var comando = conexao.CreateCommand();
        comando.CommandText = $"SELECT name FROM pragma_table_info('{tabela}')";
        using var leitor = comando.ExecuteReader();
        var colunas = new List<string>();
        while (leitor.Read())
            colunas.Add(leitor.GetString(0));
        return colunas;
    }
}
