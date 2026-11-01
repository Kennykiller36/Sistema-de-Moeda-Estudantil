using MoedaEstudantil.Dominio;

namespace MoedaEstudantil.Persistencia.Repositorios;

public interface IRepositorio<T> where T : class
{
    Task AdicionarAsync(T entidade, CancellationToken cancelamento = default);
    void Remover(T entidade);
}
