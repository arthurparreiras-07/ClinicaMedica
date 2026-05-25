using ClinicaMedica.Exceptions;
using ClinicaMedica.Interfaces;
using ClinicaMedica.Models;
using ClinicaMedica.Services;

namespace ClinicaMedica.UI;

public class MenuConsole
{
    private readonly IMedicoRepositorio _medicoRepo;
    private readonly IPacienteRepositorio _pacienteRepo;
    private readonly AgendamentoService _agendamento;

    public MenuConsole(IMedicoRepositorio medicoRepo, IPacienteRepositorio pacienteRepo, AgendamentoService agendamento)
    {
        _medicoRepo = medicoRepo;
        _pacienteRepo = pacienteRepo;
        _agendamento = agendamento;
    }

    public void Executar()
    {
        while (true)
        {
            MostrarMenuPrincipal();
            var opcao = Console.ReadLine()?.Trim();

            switch (opcao)
            {
                case "1": MenuMedicos(); break;
                case "2": MenuPacientes(); break;
                case "3": MenuConsultas(); break;
                case "0":
                    Console.WriteLine("\nSaindo do sistema. Até logo!");
                    return;
                default:
                    Aviso("Opção inválida.");
                    break;
            }
        }
    }

    // ── MENU PRINCIPAL ────────────────────────────────────────────────────────

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

    // ── MÉDICOS ───────────────────────────────────────────────────────────────

    private void MenuMedicos()
    {
        while (true)
        {
            Console.Clear();
            Titulo("MÉDICOS");
            Console.WriteLine("  1. Cadastrar médico");
            Console.WriteLine("  2. Listar todos");
            Console.WriteLine("  3. Buscar por especialidade");
            Console.WriteLine("  4. Buscar por CRM");
            Console.WriteLine("  0. Voltar");
            Console.Write("\nOpção: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1": CadastrarMedico(); break;
                case "2": ListarMedicos(); break;
                case "3": BuscarPorEspecialidade(); break;
                case "4": BuscarPorCrm(); break;
                case "0": return;
                default: Aviso("Opção inválida."); break;
            }
        }
    }

    private void CadastrarMedico()
    {
        Console.Clear();
        Titulo("CADASTRAR MÉDICO");
        try
        {
            var nome = Ler("Nome completo");
            var cpf = Ler("CPF (apenas dígitos)");
            var telefone = Ler("Telefone");
            var crm = Ler("CRM");
            var especialidade = Ler("Especialidade");

            var medico = new Medico(0, nome, cpf, telefone, crm, especialidade);
            _medicoRepo.Adicionar(medico);
            Sucesso($"Médico cadastrado com sucesso! ID: {medico.Id}");
        }
        catch (Exception ex)
        {
            Erro(ex.Message);
        }
        Pausar();
    }

    private void ListarMedicos()
    {
        Console.Clear();
        Titulo("LISTA DE MÉDICOS");
        var lista = _medicoRepo.ListarTodos().ToList();
        if (!lista.Any()) { Console.WriteLine("Nenhum médico cadastrado."); }
        else lista.ForEach(m => Console.WriteLine($"  [{m.Id,3}] {m.ExibirInformacoes()}"));
        Pausar();
    }

    private void BuscarPorEspecialidade()
    {
        Console.Clear();
        var especialidade = Ler("Especialidade");
        var resultado = _medicoRepo.BuscarPorEspecialidade(especialidade).ToList();
        Titulo($"RESULTADO — \"{especialidade}\"");
        if (!resultado.Any()) Console.WriteLine("Nenhum médico encontrado.");
        else resultado.ForEach(m => Console.WriteLine($"  [{m.Id,3}] {m.ExibirInformacoes()}"));
        Pausar();
    }

    private void BuscarPorCrm()
    {
        Console.Clear();
        var crm = Ler("CRM");
        var medico = _medicoRepo.BuscarPorCrm(crm);
        if (medico == null) Aviso("Médico não encontrado.");
        else Console.WriteLine($"\n  {medico.ExibirInformacoes()}");
        Pausar();
    }

    // ── PACIENTES ─────────────────────────────────────────────────────────────

    private void MenuPacientes()
    {
        while (true)
        {
            Console.Clear();
            Titulo("PACIENTES");
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
                default: Aviso("Opção inválida."); break;
            }
        }
    }

    private void CadastrarPaciente()
    {
        Console.Clear();
        Titulo("CADASTRAR PACIENTE");
        try
        {
            var nome = Ler("Nome completo");
            var cpf = Ler("CPF (apenas dígitos)");
            var telefone = Ler("Telefone");

            DateTime dataNasc;
            while (true)
            {
                var input = Ler("Data de nascimento (dd/MM/yyyy)");
                if (DateTime.TryParseExact(input, "dd/MM/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out dataNasc))
                    break;
                Erro("Formato inválido. Use dd/MM/yyyy.");
            }

            Console.Write("Convênio (Enter = Particular): ");
            var convenio = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(convenio)) convenio = "Particular";

            var paciente = new Paciente(0, nome, cpf, telefone, dataNasc, convenio);
            _pacienteRepo.Adicionar(paciente);
            Sucesso($"Paciente cadastrado com sucesso! ID: {paciente.Id}");
        }
        catch (Exception ex)
        {
            Erro(ex.Message);
        }
        Pausar();
    }

    private void ListarPacientes()
    {
        Console.Clear();
        Titulo("LISTA DE PACIENTES");
        var lista = _pacienteRepo.ListarTodos().ToList();
        if (!lista.Any()) Console.WriteLine("Nenhum paciente cadastrado.");
        else lista.ForEach(p => Console.WriteLine($"  [{p.Id,3}] {p.ExibirInformacoes()}"));
        Pausar();
    }

    private void BuscarPorCpf()
    {
        Console.Clear();
        var cpf = Ler("CPF");
        var paciente = _pacienteRepo.BuscarPorCpf(cpf);
        if (paciente == null) Aviso("Paciente não encontrado.");
        else Console.WriteLine($"\n  {paciente.ExibirInformacoes()}");
        Pausar();
    }

    // ── CONSULTAS ─────────────────────────────────────────────────────────────

    private void MenuConsultas()
    {
        while (true)
        {
            Console.Clear();
            Titulo("CONSULTAS");
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
                default: Aviso("Opção inválida."); break;
            }
        }
    }

    private void AgendarConsulta()
    {
        Console.Clear();
        Titulo("AGENDAR CONSULTA");
        try
        {
            Console.Write("  Filtrar por especialidade (Enter para listar todos): ");
            var filtro = Console.ReadLine()?.Trim();
            ListarMedicosInline(string.IsNullOrEmpty(filtro) ? null : filtro);
            var medicoId = LerInt("ID do médico");

            ListarPacientesInline();
            var pacienteId = LerInt("ID do paciente");

            DateTime dataHora;
            while (true)
            {
                var input = Ler("Data e hora (dd/MM/yyyy HH:mm)");
                if (DateTime.TryParseExact(input, "dd/MM/yyyy HH:mm",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out dataHora))
                    break;
                Erro("Formato inválido. Use dd/MM/yyyy HH:mm.");
            }

            Console.Write("Observações (opcional): ");
            var obs = Console.ReadLine() ?? string.Empty;

            var consulta = _agendamento.Agendar(medicoId, pacienteId, dataHora, obs);
            Sucesso($"Consulta agendada com sucesso! ID: #{consulta.Id:D4}");
        }
        catch (ConsultaConflitanteException ex) { Erro($"Conflito: {ex.Message}"); }
        catch (LimiteConsultasDiariasException ex) { Erro(ex.Message); }
        catch (Exception ex) { Erro(ex.Message); }
        Pausar();
    }

    private void CancelarConsulta()
    {
        Console.Clear();
        Titulo("CANCELAR CONSULTA");
        try
        {
            var id = LerInt("ID da consulta");
            _agendamento.Cancelar(id);
            Sucesso("Consulta cancelada com sucesso.");
        }
        catch (Exception ex) { Erro(ex.Message); }
        Pausar();
    }

    private void MarcarRealizada()
    {
        Console.Clear();
        Titulo("MARCAR COMO REALIZADA");
        try
        {
            var id = LerInt("ID da consulta");
            _agendamento.MarcarRealizada(id);
            Sucesso("Consulta marcada como realizada.");

            Console.Write("\n  Registrar diagnóstico agora? (s/N): ");
            if (Console.ReadLine()?.Trim().Equals("s", StringComparison.OrdinalIgnoreCase) == true)
            {
                var diagnostico = Ler("Diagnóstico");
                if (!string.IsNullOrEmpty(diagnostico))
                {
                    _agendamento.RegistrarDiagnostico(id, diagnostico);
                    Console.Write("\n  Adicionar prescrições? (s/N): ");
                    if (Console.ReadLine()?.Trim().Equals("s", StringComparison.OrdinalIgnoreCase) == true)
                        AdicionarPrescricoesLoop(id);
                }
            }
        }
        catch (Exception ex) { Erro(ex.Message); }
        Pausar();
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
            Erro("Data inválida.");
            Pausar();
            return;
        }

        Titulo($"CONSULTAS DE {data:dd/MM/yyyy}");
        var lista = _agendamento.ListarPorData(data).ToList();
        ExibirConsultas(lista);
        Pausar();
    }

    private void HistoricoPaciente()
    {
        Console.Clear();
        Titulo("PRONTUÁRIO DO PACIENTE");
        try
        {
            ListarPacientesInline();
            var id = LerInt("ID do paciente");
            var paciente = _pacienteRepo.BuscarPorId(id)
                ?? throw new KeyNotFoundException("Paciente não encontrado.");

            Console.WriteLine($"\n  Paciente: {paciente.ExibirInformacoes()}\n");
            var lista = _agendamento.ListarPorPaciente(id).ToList();
            ExibirProntuario(lista);
        }
        catch (Exception ex) { Erro(ex.Message); }
        Pausar();
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
                c.Prescricoes.ForEach(p => Console.WriteLine($"  │    • {p}"));
            }
            Console.WriteLine("  └─");
        }
    }

    private void AgendaMedico()
    {
        Console.Clear();
        Titulo("AGENDA DO MÉDICO");
        try
        {
            ListarMedicosInline();
            var id = LerInt("ID do médico");
            var medico = _medicoRepo.BuscarPorId(id)
                ?? throw new KeyNotFoundException("Médico não encontrado.");

            Console.WriteLine($"\n  {medico.ExibirInformacoes()}\n");
            var lista = _agendamento.ListarPorMedico(id).ToList();
            ExibirConsultas(lista);
        }
        catch (Exception ex) { Erro(ex.Message); }
        Pausar();
    }

    private void RegistrarDiagnosticoPrescricao()
    {
        Console.Clear();
        Titulo("REGISTRAR DIAGNÓSTICO / PRESCRIÇÃO");
        try
        {
            var id = LerInt("ID da consulta (deve estar como Realizada)");
            var diagnostico = Ler("Diagnóstico");
            if (!string.IsNullOrEmpty(diagnostico))
                _agendamento.RegistrarDiagnostico(id, diagnostico);

            Console.Write("\n  Adicionar prescrições? (s/N): ");
            if (Console.ReadLine()?.Trim().Equals("s", StringComparison.OrdinalIgnoreCase) == true)
                AdicionarPrescricoesLoop(id);

            Sucesso("Prontuário atualizado.");
        }
        catch (Exception ex) { Erro(ex.Message); }
        Pausar();
    }

    private void AdicionarPrescricoesLoop(int consultaId)
    {
        while (true)
        {
            Console.WriteLine();
            var medicamento = Ler("Medicamento (Enter para finalizar)");
            if (string.IsNullOrEmpty(medicamento)) break;

            var dosagem = Ler("Dosagem");
            Console.Write("  Instruções (opcional): ");
            var instrucoes = Console.ReadLine()?.Trim() ?? string.Empty;

            _agendamento.AdicionarPrescricao(consultaId, new Prescricao(medicamento, dosagem, instrucoes));
            Sucesso("Prescrição adicionada.");
        }
    }

    // ── HELPERS ───────────────────────────────────────────────────────────────

    private void ExibirConsultas(List<Consulta> lista)
    {
        if (!lista.Any()) { Console.WriteLine("  Nenhuma consulta encontrada."); return; }

        Console.WriteLine($"  {"ID",-8} {"Data/Hora",-20} {"Médico",-6} {"Paciente",-8} Status");
        Console.WriteLine(new string('-', 60));
        foreach (var c in lista)
        {
            var medico = _medicoRepo.BuscarPorId(c.MedicoId);
            var paciente = _pacienteRepo.BuscarPorId(c.PacienteId);
            Console.WriteLine($"  #{c.Id:D4}    {c.DataHora:dd/MM/yyyy HH:mm}   " +
                              $"{medico?.Nome ?? "?",-15} {paciente?.Nome ?? "?",-15} {c.Status}");
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

    private static string Ler(string campo)
    {
        Console.Write($"  {campo}: ");
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }

    private static int LerInt(string campo)
    {
        while (true)
        {
            Console.Write($"  {campo}: ");
            if (int.TryParse(Console.ReadLine(), out var valor)) return valor;
            Erro("Valor inválido. Informe um número inteiro.");
        }
    }

    private static void Titulo(string texto)
    {
        Console.WriteLine($"\n  ── {texto} ──\n");
    }

    private static void Sucesso(string msg) => Console.WriteLine($"\n  ✓ {msg}");
    private static void Erro(string msg) => Console.WriteLine($"\n  ✗ Erro: {msg}");
    private static void Aviso(string msg) => Console.WriteLine($"\n  ! {msg}");

    private static void Pausar()
    {
        Console.Write("\n  Pressione qualquer tecla para continuar...");
        Console.ReadKey();
    }
}
