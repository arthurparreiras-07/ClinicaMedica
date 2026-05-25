using ClinicaMedica.Backend.Consultas.Services;
using ClinicaMedica.Backend.Medicos.Interfaces;
using ClinicaMedica.Backend.Pacientes.Interfaces;
using ClinicaMedica.UI.TextUI.Consultas;
using ClinicaMedica.UI.TextUI.Medicos;
using ClinicaMedica.UI.TextUI.Pacientes;
using ClinicaMedica.UI.TextUI.Shared;

namespace ClinicaMedica.UI.TextUI;

public class MenuConsole
{
    private readonly MedicosMenu   _medicos;
    private readonly PacientesMenu _pacientes;
    private readonly ConsultasMenu _consultas;

    public MenuConsole(IMedicoRepositorio medicoRepo, IPacienteRepositorio pacienteRepo, AgendamentoService agendamento)
    {
        _medicos   = new MedicosMenu(medicoRepo);
        _pacientes = new PacientesMenu(pacienteRepo);
        _consultas = new ConsultasMenu(agendamento, medicoRepo, pacienteRepo);
    }

    public void Executar()
    {
        while (true)
        {
            MostrarMenuPrincipal();
            switch (Console.ReadLine()?.Trim())
            {
                case "1": _medicos.Executar();   break;
                case "2": _pacientes.Executar(); break;
                case "3": _consultas.Executar(); break;
                case "0":
                    Console.WriteLine("\nSaindo do sistema. Até logo!");
                    return;
                default:
                    ConsoleHelper.Aviso("Opção inválida.");
                    break;
            }
        }
    }

    private static void MostrarMenuPrincipal()
    {
        Console.Clear();
        Console.WriteLine("╔════════════════════════════════════╗");
        Console.WriteLine("║      CLÍNICA MÉDICA — SISTEMA      ║");
        Console.WriteLine("╠════════════════════════════════════╣");
        Console.WriteLine("║  1. Médicos                        ║");
        Console.WriteLine("║  2. Pacientes                      ║");
        Console.WriteLine("║  3. Consultas                      ║");
        Console.WriteLine("║  0. Sair                           ║");
        Console.WriteLine("╚════════════════════════════════════╝");
        Console.Write("\nOpção: ");
    }
}
