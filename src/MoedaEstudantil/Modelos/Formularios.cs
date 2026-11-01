using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MoedaEstudantil.Modelos;

public class AlunoFormulario : IValidatableObject
{
    public long? Id { get; set; }

    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(160, MinimumLength = 3, ErrorMessage = "Informe o nome completo.")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string? Senha { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirmar senha")]
    public string? ConfirmarSenha { get; set; }

    [Required(ErrorMessage = "Informe o CPF.")]
    [Display(Name = "CPF")]
    public string Cpf { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o RG.")]
    [StringLength(20, MinimumLength = 4, ErrorMessage = "Informe o RG.")]
    [Display(Name = "RG")]
    public string Rg { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o endereço.")]
    [StringLength(240, MinimumLength = 5, ErrorMessage = "Informe o endereço.")]
    [Display(Name = "Endereço")]
    public string Endereco { get; set; } = string.Empty;

    [Range(1, long.MaxValue, ErrorMessage = "Selecione a instituição de ensino.")]
    [Display(Name = "Instituição de ensino")]
    public long InstituicaoId { get; set; }

    [Required(ErrorMessage = "Informe o curso.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Informe o curso.")]
    [Display(Name = "Curso")]
    public string Curso { get; set; } = string.Empty;

    public IEnumerable<SelectListItem> Instituicoes { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var erro in ValidacaoSenha.Avaliar(Id, Senha, ConfirmarSenha))
            yield return erro;

        if (!string.IsNullOrWhiteSpace(Cpf) && !Dominio.Documento.CpfValido(Cpf))
            yield return new ValidationResult("Informe um CPF válido.", [nameof(Cpf)]);
    }
}

public class EmpresaFormulario : IValidatableObject
{
    public long? Id { get; set; }

    [Required(ErrorMessage = "Informe o nome fantasia.")]
    [StringLength(160, MinimumLength = 2, ErrorMessage = "Informe o nome fantasia.")]
    [Display(Name = "Nome fantasia")]
    public string NomeFantasia { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string? Senha { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirmar senha")]
    public string? ConfirmarSenha { get; set; }

    [Required(ErrorMessage = "Informe o CNPJ.")]
    [Display(Name = "CNPJ")]
    public string Cnpj { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var erro in ValidacaoSenha.Avaliar(Id, Senha, ConfirmarSenha))
            yield return erro;

        if (!string.IsNullOrWhiteSpace(Cnpj) && !Dominio.Documento.CnpjValido(Cnpj))
            yield return new ValidationResult("Informe um CNPJ válido.", [nameof(Cnpj)]);
    }
}

public class LoginFormulario
{
    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Senha { get; set; } = string.Empty;
}

internal static class ValidacaoSenha
{
    public static IEnumerable<ValidationResult> Avaliar(long? id, string? senha, string? confirmacao)
    {
        var criando = id is null or 0;
        if (criando && string.IsNullOrWhiteSpace(senha))
        {
            yield return new ValidationResult("Informe uma senha.", [nameof(AlunoFormulario.Senha)]);
            yield break;
        }

        if (string.IsNullOrWhiteSpace(senha))
            yield break;

        if (senha.Length < 6)
            yield return new ValidationResult("A senha deve ter pelo menos 6 caracteres.", [nameof(AlunoFormulario.Senha)]);

        if (senha != confirmacao)
            yield return new ValidationResult("A confirmação não confere com a senha.", [nameof(AlunoFormulario.ConfirmarSenha)]);
    }
}
