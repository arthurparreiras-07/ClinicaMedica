using ClinicaMedica.Database.Sqlite;
using ClinicaMedica.Backend.Medicos.Interfaces;
using ClinicaMedica.Backend.Medicos.Models;
using Microsoft.Data.Sqlite;

namespace ClinicaMedica.Backend.Medicos.Repositories;

public class MedicoRepositorioSql : RepositorioBD<Medico>, IMedicoRepositorio
{
    public MedicoRepositorioSql(ConexaoBanco banco) : base(banco) { }

    public override void Adicionar(Medico medico)
    {
        if (BuscarPorCrm(medico.Crm) != null)
            throw new InvalidOperationException($"Já existe um médico com o CRM '{medico.Crm}'.");

        const string sql = @"
            INSERT INTO Medicos (Nome, Cpf, Telefone, Crm, Especialidade)
            VALUES (@Nome, @Cpf, @Telefone, @Crm, @Especialidade)";

        using var conn = _banco.CriarConexao();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Nome", medico.Nome);
        cmd.Parameters.AddWithValue("@Cpf", medico.Cpf);
        cmd.Parameters.AddWithValue("@Telefone", medico.Telefone);
        cmd.Parameters.AddWithValue("@Crm", medico.Crm);
        cmd.Parameters.AddWithValue("@Especialidade", medico.Especialidade);
        cmd.ExecuteNonQuery();
        medico.Id = ObterUltimoId(conn);
    }

    public override Medico? BuscarPorId(int id)
    {
        const string sql = @"
            SELECT Id, Nome, Cpf, Telefone, Crm, Especialidade
            FROM Medicos WHERE Id = @Id";
        using var conn = _banco.CriarConexao();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? LerMedico(reader) : null;
    }

    public override IEnumerable<Medico> ListarTodos()
    {
        const string sql = @"
            SELECT Id, Nome, Cpf, Telefone, Crm, Especialidade
            FROM Medicos ORDER BY Nome";
        using var conn = _banco.CriarConexao();
        using var cmd = new SqliteCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        var lista = new List<Medico>();
        while (reader.Read())
            lista.Add(LerMedico(reader));
        return lista;
    }

    public override void Atualizar(Medico medico)
    {
        const string sql = @"
            UPDATE Medicos
            SET Nome = @Nome, Cpf = @Cpf, Telefone = @Telefone,
                Crm = @Crm, Especialidade = @Especialidade
            WHERE Id = @Id";
        using var conn = _banco.CriarConexao();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Nome", medico.Nome);
        cmd.Parameters.AddWithValue("@Cpf", medico.Cpf);
        cmd.Parameters.AddWithValue("@Telefone", medico.Telefone);
        cmd.Parameters.AddWithValue("@Crm", medico.Crm);
        cmd.Parameters.AddWithValue("@Especialidade", medico.Especialidade);
        cmd.Parameters.AddWithValue("@Id", medico.Id);
        if (cmd.ExecuteNonQuery() == 0)
            throw new KeyNotFoundException($"Médico ID {medico.Id} não encontrado.");
    }

    public override void Remover(int id)
    {
        const string sql = "DELETE FROM Medicos WHERE Id = @Id";
        using var conn = _banco.CriarConexao();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        if (cmd.ExecuteNonQuery() == 0)
            throw new KeyNotFoundException($"Médico ID {id} não encontrado.");
    }

    public Medico? BuscarPorCrm(string crm)
    {
        const string sql = @"
            SELECT Id, Nome, Cpf, Telefone, Crm, Especialidade
            FROM Medicos WHERE UPPER(Crm) = UPPER(@Crm)";
        using var conn = _banco.CriarConexao();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Crm", crm.Trim());
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? LerMedico(reader) : null;
    }

    public IEnumerable<Medico> BuscarPorEspecialidade(string especialidade)
    {
        const string sql = @"
            SELECT Id, Nome, Cpf, Telefone, Crm, Especialidade
            FROM Medicos
            WHERE Especialidade LIKE '%' || @Especialidade || '%'
            ORDER BY Nome";
        using var conn = _banco.CriarConexao();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Especialidade", especialidade.Trim());
        using var reader = cmd.ExecuteReader();
        var lista = new List<Medico>();
        while (reader.Read())
            lista.Add(LerMedico(reader));
        return lista;
    }

    private static Medico LerMedico(SqliteDataReader r) =>
        new(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetString(3), r.GetString(4), r.GetString(5));
}
