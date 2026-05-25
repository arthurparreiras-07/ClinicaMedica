using ClinicaMedica.Repositories;
using ClinicaMedica.Services;
using ClinicaMedica.UI;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var dataDir = Path.Combine(Path.GetDirectoryName(GetSourceDir())!, "data");

static string GetSourceDir([System.Runtime.CompilerServices.CallerFilePath] string path = "") => path;

var medicoRepo    = new MedicoRepositorio(Path.Combine(dataDir, "medicos.json"));
var pacienteRepo  = new PacienteRepositorio(Path.Combine(dataDir, "pacientes.json"));
var consultaRepo  = new ConsultaRepositorio(Path.Combine(dataDir, "consultas.json"));

var agendamento = new AgendamentoService(consultaRepo, medicoRepo, pacienteRepo);

var menu = new MenuConsole(medicoRepo, pacienteRepo, agendamento);
menu.Executar();
