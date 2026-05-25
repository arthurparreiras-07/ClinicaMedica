using ClinicaMedica.Exceptions;
using ClinicaMedica.Interfaces;
using ClinicaMedica.Models;

namespace ClinicaMedica.Services;

public class AgendamentoService
{
    private const int LimiteConsultasPorDia = 10;

    private readonly IConsultaRepositorio _consultaRepo;
    private readonly IMedicoRepositorio _medicoRepo;
    private readonly IPacienteRepositorio _pacienteRepo;

    public AgendamentoService(
        IConsultaRepositorio consultaRepo,
        IMedicoRepositorio medicoRepo,
        IPacienteRepositorio pacienteRepo)
    {
        _consultaRepo = consultaRepo;
        _medicoRepo = medicoRepo;
        _pacienteRepo = pacienteRepo;
    }

    public Consulta Agendar(int medicoId, int pacienteId, DateTime dataHora, string observacoes = "")
    {
        _ = _medicoRepo.BuscarPorId(medicoId)
            ?? throw new KeyNotFoundException($"Médico ID {medicoId} não encontrado.");

        _ = _pacienteRepo.BuscarPorId(pacienteId)
            ?? throw new KeyNotFoundException($"Paciente ID {pacienteId} não encontrado.");

        var consultasNoDia = _consultaRepo.ContarConsultasAtivasMedicoNoDia(medicoId, dataHora);
        if (consultasNoDia >= LimiteConsultasPorDia)
            throw new LimiteConsultasDiariasException(medicoId, dataHora);

        if (_consultaRepo.PacienteTemConsultaComMedicoNoDia(medicoId, pacienteId, dataHora))
            throw new ConsultaConflitanteException(
                $"Paciente ID {pacienteId} já possui consulta com este médico no dia {dataHora:dd/MM/yyyy}.");

        var consulta = new Consulta(0, medicoId, pacienteId, dataHora, observacoes);
        _consultaRepo.Adicionar(consulta);
        return consulta;
    }

    public void Cancelar(int consultaId)
    {
        var consulta = _consultaRepo.BuscarPorId(consultaId)
            ?? throw new KeyNotFoundException($"Consulta ID {consultaId} não encontrada.");

        consulta.Cancelar();
        _consultaRepo.Atualizar(consulta);
    }

    public void MarcarRealizada(int consultaId)
    {
        var consulta = _consultaRepo.BuscarPorId(consultaId)
            ?? throw new KeyNotFoundException($"Consulta ID {consultaId} não encontrada.");

        consulta.MarcarRealizada();
        _consultaRepo.Atualizar(consulta);
    }

    public IEnumerable<Consulta> ListarPorMedico(int medicoId) =>
        _consultaRepo.BuscarPorMedico(medicoId).OrderBy(c => c.DataHora);

    public IEnumerable<Consulta> ListarPorPaciente(int pacienteId) =>
        _consultaRepo.BuscarPorPaciente(pacienteId).OrderBy(c => c.DataHora);

    public IEnumerable<Consulta> ListarPorData(DateTime data) =>
        _consultaRepo.BuscarPorData(data).OrderBy(c => c.DataHora);

    public IEnumerable<Consulta> ListarTodas() =>
        _consultaRepo.ListarTodos().OrderBy(c => c.DataHora);

    public void RegistrarDiagnostico(int consultaId, string diagnostico)
    {
        var consulta = _consultaRepo.BuscarPorId(consultaId)
            ?? throw new KeyNotFoundException($"Consulta ID {consultaId} não encontrada.");
        consulta.RegistrarDiagnostico(diagnostico);
        _consultaRepo.Atualizar(consulta);
    }

    public void AdicionarPrescricao(int consultaId, Prescricao prescricao)
    {
        var consulta = _consultaRepo.BuscarPorId(consultaId)
            ?? throw new KeyNotFoundException($"Consulta ID {consultaId} não encontrada.");
        consulta.AdicionarPrescricao(prescricao);
        _consultaRepo.Atualizar(consulta);
    }
}
