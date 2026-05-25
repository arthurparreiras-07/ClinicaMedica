namespace ClinicaMedica.Backend.Core.Repositories;

public interface IRepositorio<T>
{
    void Adicionar(T entidade);
    T? BuscarPorId(int id);
    IEnumerable<T> ListarTodos();
    void Atualizar(T entidade);
    void Remover(int id);
    void Salvar();
}
