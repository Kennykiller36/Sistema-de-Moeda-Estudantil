namespace MoedaEstudantil.Persistencia;

public interface IUnidadeDeTrabalho
{
    Task SalvarAsync(CancellationToken cancelamento = default);
}
