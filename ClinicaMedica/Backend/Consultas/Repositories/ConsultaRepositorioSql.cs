using ClinicaMedica.Database;
using ClinicaMedica.Interfaces;
using ClinicaMedica.Models;
using Microsoft.Data.Sqlite;

namespace ClinicaMedica.Repositories;

public class ConsultaRepositorioSql : RepositorioBD<Consulta>, IConsultaRepositorio
{
    public ConsultaRepositorioSql(ConexaoBanco banco) : base(banco) { }

    public override void Adicionar(Consulta consulta)
    {
        const string sql = @"
            INSERT INTO Consultas (MedicoId, PacienteId, DataHora, Status, Observacoes, Diagnostico)
            VALUES (@MedicoId, @PacienteId, @DataHora, @Status, @Observacoes, @Diagnostico)";

        using var conn = _banco.CriarConexao();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MedicoId", consulta.MedicoId);
        cmd.Parameters.AddWithValue("@PacienteId", consulta.PacienteId);
        cmd.Parameters.AddWithValue("@DataHora", consulta.DataHora.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.Parameters.AddWithValue("@Status", consulta.Status.ToString());
        cmd.Parameters.AddWithValue("@Observacoes", consulta.Observacoes);
        cmd.Parameters.AddWithValue("@Diagnostico", consulta.Diagnostico);
        cmd.ExecuteNonQuery();
        consulta.Id = ObterUltimoId(conn);

        foreach (var p in consulta.Prescricoes)
            InserirPrescricao(conn, consulta.Id, p);
    }

    public override Consulta? BuscarPorId(int id)
    {
        const string sql = @"
            SELECT Id, MedicoId, PacienteId, DataHora, Status, Observacoes, Diagnostico
            FROM Consultas WHERE Id = @Id";
        using var conn = _banco.CriarConexao();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        Consulta? consulta;
        using (var reader = cmd.ExecuteReader())
            consulta = reader.Read() ? LerConsulta(reader) : null;
        if (consulta != null)
            consulta.Prescricoes = CarregarPrescricoes(conn, consulta.Id);
        return consulta;
    }

    public override IEnumerable<Consulta> ListarTodos()
    {
        const string sqlC = @"
            SELECT Id, MedicoId, PacienteId, DataHora, Status, Observacoes, Diagnostico
            FROM Consultas ORDER BY DataHora";
        const string sqlP = @"
            SELECT ConsultaId, Medicamento, Dosagem, Instrucoes
            FROM Prescricoes ORDER BY ConsultaId, Id";

        using var conn = _banco.CriarConexao();
        var consultas = new List<Consulta>();
        using (var cmd = new SqliteCommand(sqlC, conn))
        using (var reader = cmd.ExecuteReader())
            while (reader.Read())
                consultas.Add(LerConsulta(reader));

        var prescricoes = new Dictionary<int, List<Prescricao>>();
        using (var cmd = new SqliteCommand(sqlP, conn))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var cId = reader.GetInt32(0);
                if (!prescricoes.TryGetValue(cId, out var ps))
                    prescricoes[cId] = ps = new List<Prescricao>();
                ps.Add(new Prescricao(reader.GetString(1), reader.GetString(2), reader.GetString(3)));
            }
        }

        foreach (var c in consultas)
            c.Prescricoes = prescricoes.TryGetValue(c.Id, out var lista) ? lista : new List<Prescricao>();

        return consultas;
    }

    public override void Atualizar(Consulta consulta)
    {
        const string sql = @"
            UPDATE Consultas
            SET Status = @Status, Observacoes = @Observacoes, Diagnostico = @Diagnostico
            WHERE Id = @Id";

        using var conn = _banco.CriarConexao();
        using (var cmd = new SqliteCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@Status", consulta.Status.ToString());
            cmd.Parameters.AddWithValue("@Observacoes", consulta.Observacoes);
            cmd.Parameters.AddWithValue("@Diagnostico", consulta.Diagnostico);
            cmd.Parameters.AddWithValue("@Id", consulta.Id);
            if (cmd.ExecuteNonQuery() == 0)
                throw new KeyNotFoundException($"Consulta ID {consulta.Id} não encontrada.");
        }

        using (var del = new SqliteCommand("DELETE FROM Prescricoes WHERE ConsultaId = @Id", conn))
        {
            del.Parameters.AddWithValue("@Id", consulta.Id);
            del.ExecuteNonQuery();
        }

        foreach (var p in consulta.Prescricoes)
            InserirPrescricao(conn, consulta.Id, p);
    }

    public override void Remover(int id)
    {
        const string sql = "DELETE FROM Consultas WHERE Id = @Id";
        using var conn = _banco.CriarConexao();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        if (cmd.ExecuteNonQuery() == 0)
            throw new KeyNotFoundException($"Consulta ID {id} não encontrada.");
    }

    public IEnumerable<Consulta> BuscarPorMedico(int medicoId) =>
        BuscarComFiltro("WHERE MedicoId = @P", medicoId);

    public IEnumerable<Consulta> BuscarPorPaciente(int pacienteId) =>
        BuscarComFiltro("WHERE PacienteId = @P", pacienteId);

    public IEnumerable<Consulta> BuscarPorData(DateTime data)
    {
        const string sql = @"
            SELECT Id, MedicoId, PacienteId, DataHora, Status, Observacoes, Diagnostico
            FROM Consultas
            WHERE DATE(DataHora) = @Data
            ORDER BY DataHora";
        using var conn = _banco.CriarConexao();
        var lista = new List<Consulta>();
        using (var cmd = new SqliteCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@Data", data.ToString("yyyy-MM-dd"));
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                lista.Add(LerConsulta(reader));
        }
        foreach (var c in lista)
            c.Prescricoes = CarregarPrescricoes(conn, c.Id);
        return lista;
    }

    public int ContarConsultasAtivasMedicoNoDia(int medicoId, DateTime data)
    {
        const string sql = @"
            SELECT COUNT(*) FROM Consultas
            WHERE MedicoId = @MedicoId
              AND DATE(DataHora) = @Data
              AND Status != 'Cancelada'";
        using var conn = _banco.CriarConexao();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MedicoId", medicoId);
        cmd.Parameters.AddWithValue("@Data", data.ToString("yyyy-MM-dd"));
        return (int)(long)cmd.ExecuteScalar()!;
    }

    public bool PacienteTemConsultaComMedicoNoDia(int medicoId, int pacienteId, DateTime data)
    {
        const string sql = @"
            SELECT COUNT(*) FROM Consultas
            WHERE MedicoId = @MedicoId
              AND PacienteId = @PacienteId
              AND DATE(DataHora) = @Data
              AND Status != 'Cancelada'";
        using var conn = _banco.CriarConexao();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MedicoId", medicoId);
        cmd.Parameters.AddWithValue("@PacienteId", pacienteId);
        cmd.Parameters.AddWithValue("@Data", data.ToString("yyyy-MM-dd"));
        return (int)(long)cmd.ExecuteScalar()! > 0;
    }

    private IEnumerable<Consulta> BuscarComFiltro(string whereClause, int parametro)
    {
        var sqlText = $@"
            SELECT Id, MedicoId, PacienteId, DataHora, Status, Observacoes, Diagnostico
            FROM Consultas {whereClause} ORDER BY DataHora";
        using var conn = _banco.CriarConexao();
        var lista = new List<Consulta>();
        using (var cmd = new SqliteCommand(sqlText, conn))
        {
            cmd.Parameters.AddWithValue("@P", parametro);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                lista.Add(LerConsulta(reader));
        }
        foreach (var c in lista)
            c.Prescricoes = CarregarPrescricoes(conn, c.Id);
        return lista;
    }

    private static List<Prescricao> CarregarPrescricoes(SqliteConnection conn, int consultaId)
    {
        const string sql = @"
            SELECT Medicamento, Dosagem, Instrucoes
            FROM Prescricoes WHERE ConsultaId = @Id ORDER BY Id";
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", consultaId);
        using var reader = cmd.ExecuteReader();
        var lista = new List<Prescricao>();
        while (reader.Read())
            lista.Add(new Prescricao(reader.GetString(0), reader.GetString(1), reader.GetString(2)));
        return lista;
    }

    private static void InserirPrescricao(SqliteConnection conn, int consultaId, Prescricao p)
    {
        const string sql = @"
            INSERT INTO Prescricoes (ConsultaId, Medicamento, Dosagem, Instrucoes)
            VALUES (@ConsultaId, @Medicamento, @Dosagem, @Instrucoes)";
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ConsultaId", consultaId);
        cmd.Parameters.AddWithValue("@Medicamento", p.Medicamento);
        cmd.Parameters.AddWithValue("@Dosagem", p.Dosagem);
        cmd.Parameters.AddWithValue("@Instrucoes", p.Instrucoes);
        cmd.ExecuteNonQuery();
    }

    private static Consulta LerConsulta(SqliteDataReader r) =>
        new(r.GetInt32(0), r.GetInt32(1), r.GetInt32(2),
            DateTime.Parse(r.GetString(3)),
            r.GetString(5), r.GetString(6),
            Enum.Parse<StatusConsulta>(r.GetString(4)));
}
