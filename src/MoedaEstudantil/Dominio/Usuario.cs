namespace MoedaEstudantil.Dominio;

public abstract class Usuario
{
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public TipoUsuario Tipo { get; set; }

    public bool Autenticar(string email, string senha, Func<string, string, bool> verificarSenha)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(senha))
            return false;

        return string.Equals(Email, email.Trim(), StringComparison.OrdinalIgnoreCase)
            && verificarSenha(senha, Senha);
    }
}
