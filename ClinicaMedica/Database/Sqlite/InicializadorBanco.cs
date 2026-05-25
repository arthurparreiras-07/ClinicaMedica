using Microsoft.Data.Sqlite;

namespace ClinicaMedica.Database.Sqlite;

public class InicializadorBanco
{
    private readonly ConexaoBanco _banco;

    public InicializadorBanco(ConexaoBanco banco) => _banco = banco;

    public void Inicializar()
    {
        const string ddl = @"
            CREATE TABLE IF NOT EXISTS Medicos (
                Id            INTEGER PRIMARY KEY AUTOINCREMENT,
                Nome          TEXT    NOT NULL,
                Cpf           TEXT    NOT NULL,
                Telefone      TEXT    NOT NULL,
                Crm           TEXT    NOT NULL UNIQUE,
                Especialidade TEXT    NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Pacientes (
                Id             INTEGER PRIMARY KEY AUTOINCREMENT,
                Nome           TEXT    NOT NULL,
                Cpf            TEXT    NOT NULL UNIQUE,
                Telefone       TEXT    NOT NULL,
                DataNascimento TEXT    NOT NULL,
                Convenio       TEXT    NOT NULL DEFAULT 'Particular'
            );

            CREATE TABLE IF NOT EXISTS Consultas (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                MedicoId    INTEGER NOT NULL REFERENCES Medicos(Id),
                PacienteId  INTEGER NOT NULL REFERENCES Pacientes(Id),
                DataHora    TEXT    NOT NULL,
                Status      TEXT    NOT NULL DEFAULT 'Agendada',
                Observacoes TEXT    NOT NULL DEFAULT '',
                Diagnostico TEXT    NOT NULL DEFAULT ''
            );

            CREATE TABLE IF NOT EXISTS Prescricoes (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                ConsultaId  INTEGER NOT NULL REFERENCES Consultas(Id) ON DELETE CASCADE,
                Medicamento TEXT    NOT NULL,
                Dosagem     TEXT    NOT NULL,
                Instrucoes  TEXT    NOT NULL DEFAULT ''
            );";

        using var conn = _banco.CriarConexao();
        using var cmd = new SqliteCommand(ddl, conn);
        cmd.ExecuteNonQuery();
    }
}
