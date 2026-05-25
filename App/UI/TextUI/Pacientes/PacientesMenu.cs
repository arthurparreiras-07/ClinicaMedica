using ClinicaMedica.Backend.Pacientes.Interfaces;
using ClinicaMedica.Backend.Pacientes.Models;
using ClinicaMedica.UI.TextUI.Shared;

namespace ClinicaMedica.UI.TextUI.Pacientes;

public class PacientesMenu
{
    private readonly IPacienteRepositorio _pacienteRepo;

    public PacientesMenu(IPacienteRepositorio pacienteRepo)
    {
        _pacienteRepo = pacienteRepo;
    }

    public void Executar()
    {
        while (true)
        {
            Console.Clear();
            ConsoleHelper.Titulo("PACIENTES");
            Console.WriteLine("  1. Cadastrar paciente");
            Console.WriteLine("  2. Listar todos");
            Console.WriteLine("  3. Buscar por CPF");
            Console.WriteLine("  0. Voltar");
            Console.Write("\nOpção: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1": CadastrarPaciente(); break;
                case "2": ListarPacientes(); break;
                case "3": BuscarPorCpf(); break;
                case "0": return;
                default: ConsoleHelper.Aviso("Opção inválida."); break;
            }
        }
    }

    private void CadastrarPaciente()
    {
        Console.Clear();
        ConsoleHelper.Titulo("CADASTRAR PACIENTE");
        try
        {
            var nome     = ConsoleHelper.Ler("Nome completo");
            var cpf      = ConsoleHelper.Ler("CPF (apenas dígitos)");
            var telefone = ConsoleHelper.Ler("Telefone");

            DateTime dataNasc;
            while (true)
            {
                var input = ConsoleHelper.Ler("Data de nascimento (dd/MM/yyyy)");
                if (DateTime.TryParseExact(input, "dd/MM/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out dataNasc))
                    break;
                ConsoleHelper.Erro("Formato inválido. Use dd/MM/yyyy.");
            }

            Console.Write("Convênio (Enter = Particular): ");
            var convenio = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(convenio)) convenio = "Particular";

            var paciente = new Paciente(0, nome, cpf, telefone, dataNasc, convenio);
            _pacienteRepo.Adicionar(paciente);
            ConsoleHelper.Sucesso($"Paciente cadastrado com sucesso! ID: {paciente.Id}");
        }
        catch (Exception ex) { ConsoleHelper.Erro(ex.Message); }
        ConsoleHelper.Pausar();
    }

    private void ListarPacientes()
    {
        Console.Clear();
        ConsoleHelper.Titulo("LISTA DE PACIENTES");
        var lista = _pacienteRepo.ListarTodos().ToList();
        if (!lista.Any()) Console.WriteLine("Nenhum paciente cadastrado.");
        else lista.ForEach(p => Console.WriteLine($"  [{p.Id,3}] {p.ExibirInformacoes()}"));
        ConsoleHelper.Pausar();
    }

    private void BuscarPorCpf()
    {
        Console.Clear();
        var cpf = ConsoleHelper.Ler("CPF");
        var paciente = _pacienteRepo.BuscarPorCpf(cpf);
        if (paciente == null) ConsoleHelper.Aviso("Paciente não encontrado.");
        else Console.WriteLine($"\n  {paciente.ExibirInformacoes()}");
        ConsoleHelper.Pausar();
    }
}
