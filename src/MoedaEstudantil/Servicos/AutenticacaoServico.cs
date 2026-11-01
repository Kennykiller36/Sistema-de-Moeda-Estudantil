using MoedaEstudantil.Dominio;
using MoedaEstudantil.Persistencia.Repositorios;

namespace MoedaEstudantil.Servicos;

public sealed record Sessao(long Id, string Nome, string Email, TipoUsuario Tipo);

public class AutenticacaoServico
{
    private readonly IUsuarioRepositorio _usuarios;

    public AutenticacaoServico(IUsuarioRepositorio usuarios) => _usuarios = usuarios;

    public async Task<Sessao?> EntrarAsync(string email, string senha, CancellationToken cancelamento = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(senha))
            return null;

        var usuario = await _usuarios.ObterPorEmailAsync(Normalizacao.Email(email), cancelamento);
        if (usuario is null || !usuario.Autenticar(email, senha, SenhaHasher.Verificar))
            return null;

        var nome = usuario switch
        {
            Aluno aluno => aluno.Nome,
            EmpresaParceira empresa => empresa.NomeFantasia,
            Professor professor => professor.Nome,
            _ => usuario.Email
        };

        return new Sessao(usuario.Id, nome, usuario.Email, usuario.Tipo);
    }
}
