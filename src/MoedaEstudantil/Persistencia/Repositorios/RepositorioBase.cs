using Microsoft.EntityFrameworkCore;

namespace MoedaEstudantil.Persistencia.Repositorios;

public class RepositorioBase<T> : IRepositorio<T> where T : class
{
    protected RepositorioBase(MoedaEstudantilContext contexto)
    {
        Contexto = contexto;
        Conjunto = contexto.Set<T>();
    }

    protected MoedaEstudantilContext Contexto { get; }
    protected DbSet<T> Conjunto { get; }

    public Task AdicionarAsync(T entidade, CancellationToken cancelamento = default)
        => Conjunto.AddAsync(entidade, cancelamento).AsTask();

    public void Remover(T entidade) => Conjunto.Remove(entidade);
}
