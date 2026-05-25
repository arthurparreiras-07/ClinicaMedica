using ClinicaMedica.Backend.Consultas.Exceptions;
using ClinicaMedica.Backend.Consultas.Models;
using ClinicaMedica.Backend.Consultas.Services;
using ClinicaMedica.Backend.Medicos.Interfaces;
using ClinicaMedica.Backend.Pacientes.Interfaces;
using ClinicaMedica.UI.TextUI.Shared;

namespace ClinicaMedica.UI.TextUI.Consultas;

public class ConsultasMenu
{
    private readonly AgendamentoService   _agendamento;
    private readonly IMedicoRepositorio   _medicoRepo;
    private readonly IPacienteRepositorio _pacienteRepo;

    public ConsultasMenu(AgendamentoService agendamento, IMedicoRepositorio medicoRepo, IPacienteRepositorio pacienteRepo)
    {
        _agendamento  = agendamento;
        _medicoRepo   = medicoRepo;
        _pacienteRepo = pacienteRepo;
    }

    public void Executar()
    {
        while (true)
        {
            Console.Clear();
            ConsoleHelper.Titulo("CONSULTAS");
            Console.WriteLine("  1. Agendar consulta");
            Console.WriteLine("  2. Cancelar consulta");
            Console.WriteLine("  3. Marcar como realizada");
            Console.WriteLine("  4. Consultas do dia");
            Console.WriteLine("  5. Prontuário do paciente");
            Console.WriteLine("  6. Agenda de um médico");
            Console.WriteLine("  7. Registrar diagnóstico/prescrição");
            Console.WriteLine("  0. Voltar");
            Console.Write("\nOpção: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1": AgendarConsulta(); break;
                case "2": CancelarConsulta(); break;
                case "3": MarcarRealizada(); break;
                case "4": ConsultasDoDia(); break;
                case "5": HistoricoPaciente(); break;
                case "6": AgendaMedico(); break;
                case "7": RegistrarDiagnosticoPrescricao(); break;
                case "0": return;
                default: ConsoleHelper.Aviso("Opção inválida."); break;
            }
        }
    }

    private void AgendarConsulta()
    {
        Console.Clear();
        ConsoleHelper.Titulo("AGENDAR CONSULTA");
        try
        {
            Console.Write("  Filtrar por especialidade (Enter para listar todos): ");
            var filtro = Console.ReadLine()?.Trim();
            ListarMedicosInline(string.IsNullOrEmpty(filtro) ? null : filtro);
            var medicoId = ConsoleHelper.LerInt("ID do médico");

            ListarPacientesInline();
            var pacienteId = ConsoleHelper.LerInt("ID do paciente");

            DateTime dataHora;
            while (true)
            {
                var input = ConsoleHelper.Ler("Data e hora (dd/MM/yyyy HH:mm)");
                if (DateTime.TryParseExact(input, "dd/MM/yyyy HH:mm",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out dataHora))
                    break;
                ConsoleHelper.Erro("Formato inválido. Use dd/MM/yyyy HH:mm.");
            }

            Console.Write("Observações (opcional): ");
            var obs = Console.ReadLine() ?? string.Empty;

            var consulta = _agendamento.Agendar(medicoId, pacienteId, dataHora, obs);
            ConsoleHelper.Sucesso($"Consulta agendada com sucesso! ID: #{consulta.Id:D4}");
        }
        catch (ConsultaConflitanteException ex)   { ConsoleHelper.Erro($"Conflito: {ex.Message}"); }
        catch (LimiteConsultasDiariasException ex) { ConsoleHelper.Erro(ex.Message); }
        catch (Exception ex)                       { ConsoleHelper.Erro(ex.Message); }
        ConsoleHelper.Pausar();
    }

    private void CancelarConsulta()
    {
        Console.Clear();
        ConsoleHelper.Titulo("CANCELAR CONSULTA");
        try
        {
            var id = ConsoleHelper.LerInt("ID da consulta");
            _agendamento.Cancelar(id);
            ConsoleHelper.Sucesso("Consulta cancelada com sucesso.");
        }
        catch (Exception ex) { ConsoleHelper.Erro(ex.Message); }
        ConsoleHelper.Pausar();
    }

    private void MarcarRealizada()
    {
        Console.Clear();
        ConsoleHelper.Titulo("MARCAR COMO REALIZADA");
        try
        {
            var id = ConsoleHelper.LerInt("ID da consulta");
            _agendamento.MarcarRealizada(id);
            ConsoleHelper.Sucesso("Consulta marcada como realizada.");

            Console.Write("\n  Registrar diagnóstico agora? (s/N): ");
            if (Console.ReadLine()?.Trim().Equals("s", StringComparison.OrdinalIgnoreCase) == true)
            {
                var diagnostico = ConsoleHelper.Ler("Diagnóstico");
                if (!string.IsNullOrEmpty(diagnostico))
                {
                    _agendamento.RegistrarDiagnostico(id, diagnostico);
                    Console.Write("\n  Adicionar prescrições? (s/N): ");
                    if (Console.ReadLine()?.Trim().Equals("s", StringComparison.OrdinalIgnoreCase) == true)
                        AdicionarPrescricoesLoop(id);
                }
            }
        }
        catch (Exception ex) { ConsoleHelper.Erro(ex.Message); }
        ConsoleHelper.Pausar();
    }

    private void ConsultasDoDia()
    {
        Console.Clear();
        Console.Write("Data (dd/MM/yyyy, Enter = hoje): ");
        var input = Console.ReadLine()?.Trim();
        DateTime data;
        if (string.IsNullOrEmpty(input))
            data = DateTime.Today;
        else if (!DateTime.TryParseExact(input, "dd/MM/yyyy",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out data))
        {
            ConsoleHelper.Erro("Data inválida.");
            ConsoleHelper.Pausar();
            return;
        }

        ConsoleHelper.Titulo($"CONSULTAS DE {data:dd/MM/yyyy}");
        ExibirConsultas(_agendamento.ListarPorData(data).ToList());
        ConsoleHelper.Pausar();
    }

    private void HistoricoPaciente()
    {
        Console.Clear();
        ConsoleHelper.Titulo("PRONTUÁRIO DO PACIENTE");
        try
        {
            ListarPacientesInline();
            var id = ConsoleHelper.LerInt("ID do paciente");
            var paciente = _pacienteRepo.BuscarPorId(id)
                ?? throw new KeyNotFoundException("Paciente não encontrado.");

            Console.WriteLine($"\n  Paciente: {paciente.ExibirInformacoes()}\n");
            ExibirProntuario(_agendamento.ListarPorPaciente(id).ToList());
        }
        catch (Exception ex) { ConsoleHelper.Erro(ex.Message); }
        ConsoleHelper.Pausar();
    }

    private void AgendaMedico()
    {
        Console.Clear();
        ConsoleHelper.Titulo("AGENDA DO MÉDICO");
        try
        {
            ListarMedicosInline();
            var id = ConsoleHelper.LerInt("ID do médico");
            var medico = _medicoRepo.BuscarPorId(id)
                ?? throw new KeyNotFoundException("Médico não encontrado.");

            Console.WriteLine($"\n  {medico.ExibirInformacoes()}\n");
            ExibirConsultas(_agendamento.ListarPorMedico(id).ToList());
        }
        catch (Exception ex) { ConsoleHelper.Erro(ex.Message); }
        ConsoleHelper.Pausar();
    }

    private void RegistrarDiagnosticoPrescricao()
    {
        Console.Clear();
        ConsoleHelper.Titulo("REGISTRAR DIAGNÓSTICO / PRESCRIÇÃO");
        try
        {
            var id = ConsoleHelper.LerInt("ID da consulta (deve estar como Realizada)");
            var diagnostico = ConsoleHelper.Ler("Diagnóstico");
            if (!string.IsNullOrEmpty(diagnostico))
                _agendamento.RegistrarDiagnostico(id, diagnostico);

            Console.Write("\n  Adicionar prescrições? (s/N): ");
            if (Console.ReadLine()?.Trim().Equals("s", StringComparison.OrdinalIgnoreCase) == true)
                AdicionarPrescricoesLoop(id);

            ConsoleHelper.Sucesso("Prontuário atualizado.");
        }
        catch (Exception ex) { ConsoleHelper.Erro(ex.Message); }
        ConsoleHelper.Pausar();
    }

    private void AdicionarPrescricoesLoop(int consultaId)
    {
        while (true)
        {
            Console.WriteLine();
            var medicamento = ConsoleHelper.Ler("Medicamento (Enter para finalizar)");
            if (string.IsNullOrEmpty(medicamento)) break;

            var dosagem    = ConsoleHelper.Ler("Dosagem");
            Console.Write("  Instruções (opcional): ");
            var instrucoes = Console.ReadLine()?.Trim() ?? string.Empty;

            _agendamento.AdicionarPrescricao(consultaId, new Prescricao(medicamento, dosagem, instrucoes));
            ConsoleHelper.Sucesso("Prescrição adicionada.");
        }
    }

    private void ExibirConsultas(List<Consulta> lista)
    {
        if (!lista.Any()) { Console.WriteLine("  Nenhuma consulta encontrada."); return; }

        Console.WriteLine($"  {"ID",-8} {"Data/Hora",-20} {"Médico",-6} {"Paciente",-8} Status");
        Console.WriteLine(new string('-', 60));
        foreach (var c in lista)
        {
            var medico   = _medicoRepo.BuscarPorId(c.MedicoId);
            var paciente = _pacienteRepo.BuscarPorId(c.PacienteId);
            Console.WriteLine($"  #{c.Id:D4}    {c.DataHora:dd/MM/yyyy HH:mm}   " +
                              $"{medico?.Nome ?? "?",-15} {paciente?.Nome ?? "?",-15} {c.Status}");
        }
    }

    private void ExibirProntuario(List<Consulta> lista)
    {
        if (!lista.Any()) { Console.WriteLine("  Nenhuma consulta encontrada."); return; }

        foreach (var c in lista)
        {
            var medico = _medicoRepo.BuscarPorId(c.MedicoId);
            Console.WriteLine($"\n  ┌─ #{c.Id:D4} {c.DataHora:dd/MM/yyyy HH:mm} — {medico?.Nome ?? "?"} ({medico?.Especialidade ?? "?"}) [{c.Status}]");
            if (!string.IsNullOrEmpty(c.Observacoes))
                Console.WriteLine($"  │  Obs: {c.Observacoes}");
            if (!string.IsNullOrEmpty(c.Diagnostico))
                Console.WriteLine($"  │  Diagnóstico: {c.Diagnostico}");
            if (c.Prescricoes.Count > 0)
            {
                Console.WriteLine("  │  Prescrições:");
                foreach (var p in c.Prescricoes) Console.WriteLine($"  │    • {p}");
            }
            Console.WriteLine("  └─");
        }
    }

    private void ListarMedicosInline(string? filtroEspecialidade = null)
    {
        var lista = string.IsNullOrEmpty(filtroEspecialidade)
            ? _medicoRepo.ListarTodos().ToList()
            : _medicoRepo.BuscarPorEspecialidade(filtroEspecialidade).ToList();

        if (lista.Any())
        {
            var titulo = string.IsNullOrEmpty(filtroEspecialidade)
                ? "Médicos disponíveis:"
                : $"Médicos — especialidade \"{filtroEspecialidade}\":";
            Console.WriteLine($"\n  {titulo}");
            lista.ForEach(m => Console.WriteLine($"    [{m.Id}] {m.Nome} — {m.Especialidade}"));
        }
        else
        {
            Console.WriteLine(string.IsNullOrEmpty(filtroEspecialidade)
                ? "\n  Nenhum médico cadastrado."
                : $"\n  Nenhum médico encontrado para a especialidade \"{filtroEspecialidade}\".");
        }
    }

    private void ListarPacientesInline()
    {
        var lista = _pacienteRepo.ListarTodos().ToList();
        if (lista.Any())
        {
            Console.WriteLine("\n  Pacientes cadastrados:");
            lista.ForEach(p => Console.WriteLine($"    [{p.Id}] {p.Nome}"));
        }
    }
}
