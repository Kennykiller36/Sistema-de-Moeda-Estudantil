using MoedaEstudantil.Dominio;

namespace MoedaEstudantil.Modelos;

public sealed record PaginaInicial(int? Saldo);

public sealed record VitrineVantagens(IReadOnlyList<Vantagem> Vantagens, int Saldo);

public sealed record ConfirmacaoResgate(Vantagem Vantagem, int Saldo);

public sealed record DetalhesEmpresa(EmpresaParceira Empresa, IReadOnlyList<Vantagem> Vantagens);

public sealed record ConferenciaCupons(string? Codigo, Cupom? Encontrado, IReadOnlyList<Cupom> Cupons);

public sealed record CaixaNotificacoes(string Email, IReadOnlyList<Notificacao> Notificacoes);
