using ClinicaMedica.Database;
using ClinicaMedica.Interfaces;
using ClinicaMedica.Models;
using Microsoft.Data.Sqlite;

namespace ClinicaMedica.Repositories;

public class PacienteRepositorioSql : RepositorioBD<Paciente>, IPacienteRepositorio
{
    public PacienteRepositorioSql(ConexaoBanco banco) : base(banco) { }

    public override void Adicionar(Paciente paciente)
    {
        if (BuscarPorCpf(paciente.Cpf) != null)
            throw new InvalidOperationException($"Já existe um paciente com o CPF '{paciente.Cpf}'.");

        const string sql = @"
            INSERT INTO Pacientes (Nome, Cpf, Telefone, DataNascimento, Convenio)
            VALUES (@Nome, @Cpf, @Telefone, @DataNascimento, @Convenio)";

        using var conn = _banco.CriarConexao();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Nome", paciente.Nome);
        cmd.Parameters.AddWithValue("@Cpf", paciente.Cpf);
        cmd.Parameters.AddWithValue("@Telefone", paciente.Telefone);
        cmd.Parameters.AddWithValue("@DataNascimento", paciente.DataNascimento.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@Convenio", paciente.Convenio);
        cmd.ExecuteNonQuery();
        paciente.Id = ObterUltimoId(conn);
    }

    public override Paciente? BuscarPorId(int id)
    {
        const string sql = @"
            SELECT Id, Nome, Cpf, Telefone, DataNascimento, Convenio
            FROM Pacientes WHERE Id = @Id";
        using var conn = _banco.CriarConexao();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? LerPaciente(reader) : null;
    }

    public override IEnumerable<Paciente> ListarTodos()
    {
        const string sql = @"
            SELECT Id, Nome, Cpf, Telefone, DataNascimento, Convenio
            FROM Pacientes ORDER BY Nome";
        using var conn = _banco.CriarConexao();
        using var cmd = new SqliteCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        var lista = new List<Paciente>();
        while (reader.Read())
            lista.Add(LerPaciente(reader));
        return lista;
    }

    public override void Atualizar(Paciente paciente)
    {
        const string sql = @"
            UPDATE Pacientes
            SET Nome = @Nome, Cpf = @Cpf, Telefone = @Telefone,
                DataNascimento = @DataNascimento, Convenio = @Convenio
            WHERE Id = @Id";
        using var conn = _banco.CriarConexao();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Nome", paciente.Nome);
        cmd.Parameters.AddWithValue("@Cpf", paciente.Cpf);
        cmd.Parameters.AddWithValue("@Telefone", paciente.Telefone);
        cmd.Parameters.AddWithValue("@DataNascimento", paciente.DataNascimento.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@Convenio", paciente.Convenio);
        cmd.Parameters.AddWithValue("@Id", paciente.Id);
        if (cmd.ExecuteNonQuery() == 0)
            throw new KeyNotFoundException($"Paciente ID {paciente.Id} não encontrado.");
    }

    public override void Remover(int id)
    {
        const string sql = "DELETE FROM Pacientes WHERE Id = @Id";
        using var conn = _banco.CriarConexao();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        if (cmd.ExecuteNonQuery() == 0)
            throw new KeyNotFoundException($"Paciente ID {id} não encontrado.");
    }

    public Paciente? BuscarPorCpf(string cpf)
    {
        var cpfLimpo = cpf.Replace(".", "").Replace("-", "").Trim();
        const string sql = @"
            SELECT Id, Nome, Cpf, Telefone, DataNascimento, Convenio
            FROM Pacientes WHERE Cpf = @Cpf";
        using var conn = _banco.CriarConexao();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Cpf", cpfLimpo);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? LerPaciente(reader) : null;
    }

    private static Paciente LerPaciente(SqliteDataReader r) =>
        new(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetString(3),
            DateTime.Parse(r.GetString(4)), r.GetString(5));
}
