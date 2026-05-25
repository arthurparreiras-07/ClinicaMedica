namespace ClinicaMedica.Models;

public enum StatusConsulta
{
    Agendada,
    Realizada,
    Cancelada
}

public class Consulta
{
    private DateTime _dataHora;

    public int Id { get; set; }
    public int MedicoId { get; set; }
    public int PacienteId { get; set; }

    public DateTime DataHora
    {
        get => _dataHora;
        set
        {
            if (value < DateTime.Now.AddMinutes(-1))
                throw new ArgumentException("Consulta não pode ser agendada no passado.");
            _dataHora = value;
        }
    }

    public StatusConsulta Status { get; set; } = StatusConsulta.Agendada;
    public string Observacoes { get; set; } = string.Empty;
    public string Diagnostico { get; set; } = string.Empty;
    public List<Prescricao> Prescricoes { get; set; } = new();

    public Consulta() { }

    public Consulta(int id, int medicoId, int pacienteId, DateTime dataHora, string observacoes = "")
    {
        Id = id;
        MedicoId = medicoId;
        PacienteId = pacienteId;
        DataHora = dataHora;
        Observacoes = observacoes;
    }

    public void Cancelar()
    {
        if (Status == StatusConsulta.Realizada)
            throw new InvalidOperationException("Não é possível cancelar uma consulta já realizada.");
        if (Status == StatusConsulta.Cancelada)
            throw new InvalidOperationException("A consulta já está cancelada.");
        Status = StatusConsulta.Cancelada;
    }

    public void MarcarRealizada()
    {
        if (Status == StatusConsulta.Cancelada)
            throw new InvalidOperationException("Não é possível realizar uma consulta cancelada.");
        Status = StatusConsulta.Realizada;
    }

    public void RegistrarDiagnostico(string diagnostico)
    {
        if (Status != StatusConsulta.Realizada)
            throw new InvalidOperationException("Diagnóstico só pode ser registrado em consultas realizadas.");
        if (string.IsNullOrWhiteSpace(diagnostico))
            throw new ArgumentException("Diagnóstico não pode ser vazio.");
        Diagnostico = diagnostico.Trim();
    }

    public void AdicionarPrescricao(Prescricao prescricao)
    {
        if (Status != StatusConsulta.Realizada)
            throw new InvalidOperationException("Prescrições só podem ser adicionadas em consultas realizadas.");
        Prescricoes.Add(prescricao);
    }

    public override string ToString() =>
        $"[#{Id:D4}] {DataHora:dd/MM/yyyy HH:mm} | Médico ID: {MedicoId} | Paciente ID: {PacienteId} | {Status}";
}
