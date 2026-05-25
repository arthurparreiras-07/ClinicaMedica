using ClinicaMedica.Backend.Consultas.Interfaces;
using ClinicaMedica.Backend.Consultas.Repositories;
using ClinicaMedica.Backend.Consultas.Services;
using ClinicaMedica.Database.Sqlite;
using ClinicaMedica.Backend.Medicos.Interfaces;
using ClinicaMedica.Backend.Medicos.Repositories;
using ClinicaMedica.Backend.Pacientes.Interfaces;
using ClinicaMedica.Backend.Pacientes.Repositories;
using ClinicaMedica.UI;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// ── Caminhos de dados ─────────────────────────────────────────────────────
static string GetSourceDir([System.Runtime.CompilerServices.CallerFilePath] string p = "") => p;
var baseDir    = Path.GetDirectoryName(GetSourceDir())!;
var sqliteDir  = Path.Combine(baseDir, "Database", "Sqlite");
var jsonDir    = Path.Combine(baseDir, "Database", "Json");
Directory.CreateDirectory(sqliteDir);
Directory.CreateDirectory(jsonDir);

// ── SQLite ────────────────────────────────────────────────────────────────
var banco = new ConexaoBanco($"Data Source={Path.Combine(sqliteDir, "clinica.db")}");
new InicializadorBanco(banco).Inicializar();

IMedicoRepositorio   medicoRepo   = new MedicoRepositorioSql(banco);
IPacienteRepositorio pacienteRepo = new PacienteRepositorioSql(banco);
IConsultaRepositorio consultaRepo = new ConsultaRepositorioSql(banco);

// ── Para voltar ao modo JSON, substitua as três linhas acima por: ──────────
// IMedicoRepositorio   medicoRepo   = new MedicoRepositorio(Path.Combine(jsonDir, "medicos.json"));
// IPacienteRepositorio pacienteRepo = new PacienteRepositorio(Path.Combine(jsonDir, "pacientes.json"));
// IConsultaRepositorio consultaRepo = new ConsultaRepositorio(Path.Combine(jsonDir, "consultas.json"));
// ──────────────────────────────────────────────────────────────────────────

var agendamento = new AgendamentoService(consultaRepo, medicoRepo, pacienteRepo);
var menu = new MenuConsole(medicoRepo, pacienteRepo, agendamento);
menu.Executar();
