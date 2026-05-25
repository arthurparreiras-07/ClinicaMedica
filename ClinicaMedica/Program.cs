using ClinicaMedica.Database;
using ClinicaMedica.Interfaces;
using ClinicaMedica.Repositories;
using ClinicaMedica.Services;
using ClinicaMedica.UI;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// ── Banco de dados SQLite ──────────────────────────────────────────────────
var dataDir = Path.Combine(Path.GetDirectoryName(GetSourceDir())!, "data");
static string GetSourceDir([System.Runtime.CompilerServices.CallerFilePath] string p = "") => p;

Directory.CreateDirectory(dataDir);
var dbPath = Path.Combine(dataDir, "clinica.db");

var banco = new ConexaoBanco($"Data Source={dbPath}");
new InicializadorBanco(banco).Inicializar();

IMedicoRepositorio   medicoRepo   = new MedicoRepositorioSql(banco);
IPacienteRepositorio pacienteRepo = new PacienteRepositorioSql(banco);
IConsultaRepositorio consultaRepo = new ConsultaRepositorioSql(banco);

// ── Para voltar ao modo JSON, substitua as quatro linhas acima por: ────────
// IMedicoRepositorio   medicoRepo   = new MedicoRepositorio(Path.Combine(dataDir, "medicos.json"));
// IPacienteRepositorio pacienteRepo = new PacienteRepositorio(Path.Combine(dataDir, "pacientes.json"));
// IConsultaRepositorio consultaRepo = new ConsultaRepositorio(Path.Combine(dataDir, "consultas.json"));
// ──────────────────────────────────────────────────────────────────────────

var agendamento = new AgendamentoService(consultaRepo, medicoRepo, pacienteRepo);
var menu = new MenuConsole(medicoRepo, pacienteRepo, agendamento);
menu.Executar();
