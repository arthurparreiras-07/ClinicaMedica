using ClinicaMedica.Database;
using ClinicaMedica.Interfaces;
using Microsoft.Data.Sqlite;

namespace ClinicaMedica.Repositories;

public abstract class RepositorioBD<T> : IRepositorio<T>
{
    protected readonly ConexaoBanco _banco;

    protected RepositorioBD(ConexaoBanco banco)
    {
        _banco = banco;
    }

    // No-op: SQLite persiste imediatamente em cada operação, sem cache em memória.
    public void Salvar() { }

    protected static int ObterUltimoId(SqliteConnection conn)
    {
        using var cmd = new SqliteCommand("SELECT last_insert_rowid()", conn);
        return (int)(long)cmd.ExecuteScalar()!;
    }

    public abstract void Adicionar(T entidade);
    public abstract T? BuscarPorId(int id);
    public abstract IEnumerable<T> ListarTodos();
    public abstract void Atualizar(T entidade);
    public abstract void Remover(int id);
}
