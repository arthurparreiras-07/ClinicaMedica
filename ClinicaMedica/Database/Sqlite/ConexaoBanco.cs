using Microsoft.Data.Sqlite;

namespace ClinicaMedica.Database;

public class ConexaoBanco
{
    private readonly string _connectionString;

    public ConexaoBanco(string connectionString)
    {
        _connectionString = connectionString;
    }

    public SqliteConnection CriarConexao()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        // Habilita integridade referencial (chaves estrangeiras são opt-in no SQLite)
        using var pragma = new SqliteCommand("PRAGMA foreign_keys = ON;", conn);
        pragma.ExecuteNonQuery();
        return conn;
    }
}
