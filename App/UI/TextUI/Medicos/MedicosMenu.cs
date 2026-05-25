using ClinicaMedica.Backend.Medicos.Interfaces;
using ClinicaMedica.Backend.Medicos.Models;
using ClinicaMedica.UI.TextUI.Shared;

namespace ClinicaMedica.UI.TextUI.Medicos;

public class MedicosMenu
{
    private readonly IMedicoRepositorio _medicoRepo;

    public MedicosMenu(IMedicoRepositorio medicoRepo)
    {
        _medicoRepo = medicoRepo;
    }

    public void Executar()
    {
        while (true)
        {
            Console.Clear();
            ConsoleHelper.Titulo("MÉDICOS");
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
                default: ConsoleHelper.Aviso("Opção inválida."); break;
            }
        }
    }

    private void CadastrarMedico()
    {
        Console.Clear();
        ConsoleHelper.Titulo("CADASTRAR MÉDICO");
        try
        {
            var nome          = ConsoleHelper.Ler("Nome completo");
            var cpf           = ConsoleHelper.Ler("CPF (apenas dígitos)");
            var telefone      = ConsoleHelper.Ler("Telefone");
            var crm           = ConsoleHelper.Ler("CRM");
            var especialidade = ConsoleHelper.Ler("Especialidade");

            var medico = new Medico(0, nome, cpf, telefone, crm, especialidade);
            _medicoRepo.Adicionar(medico);
            ConsoleHelper.Sucesso($"Médico cadastrado com sucesso! ID: {medico.Id}");
        }
        catch (Exception ex) { ConsoleHelper.Erro(ex.Message); }
        ConsoleHelper.Pausar();
    }

    private void ListarMedicos()
    {
        Console.Clear();
        ConsoleHelper.Titulo("LISTA DE MÉDICOS");
        var lista = _medicoRepo.ListarTodos().ToList();
        if (!lista.Any()) Console.WriteLine("Nenhum médico cadastrado.");
        else lista.ForEach(m => Console.WriteLine($"  [{m.Id,3}] {m.ExibirInformacoes()}"));
        ConsoleHelper.Pausar();
    }

    private void BuscarPorEspecialidade()
    {
        Console.Clear();
        var especialidade = ConsoleHelper.Ler("Especialidade");
        var resultado = _medicoRepo.BuscarPorEspecialidade(especialidade).ToList();
        ConsoleHelper.Titulo($"RESULTADO — \"{especialidade}\"");
        if (!resultado.Any()) Console.WriteLine("Nenhum médico encontrado.");
        else resultado.ForEach(m => Console.WriteLine($"  [{m.Id,3}] {m.ExibirInformacoes()}"));
        ConsoleHelper.Pausar();
    }

    private void BuscarPorCrm()
    {
        Console.Clear();
        var crm = ConsoleHelper.Ler("CRM");
        var medico = _medicoRepo.BuscarPorCrm(crm);
        if (medico == null) ConsoleHelper.Aviso("Médico não encontrado.");
        else Console.WriteLine($"\n  {medico.ExibirInformacoes()}");
        ConsoleHelper.Pausar();
    }
}
