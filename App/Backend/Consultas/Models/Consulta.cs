using System.Text.Json.Serialization;

namespace ClinicaMedica.Backend.Consultas.Models;

public enum StatusConsulta
{
    Agendada,
    Realizada,
    Cancelada
}

public class Consulta
{
    private DateTime _dataHora;
    private StatusConsulta _status = StatusConsulta.Agendada;
    private string _diagnostico = string.Empty;
    private readonly List<Prescricao> _prescricoes = new();

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

    public StatusConsulta Status => _status;
    public string Observacoes { get; set; } = string.Empty;
    public string Diagnostico => _diagnostico;
    public IReadOnlyList<Prescricao> Prescricoes => _prescricoes;

    public Consulta() { }

    public Consulta(int id, int medicoId, int pacienteId, DateTime dataHora, string observacoes = "")
    {
        Id = id;
        MedicoId = medicoId;
        PacienteId = pacienteId;
        DataHora = dataHora;
        Observacoes = observacoes;
    }

    // Construtor de hidratação — usado pelo JSON (via [JsonConstructor]) e pelo repositório SQL.
    // Define _dataHora diretamente para não disparar a validação de data passada em dados históricos.
    [JsonConstructor]
    public Consulta(int id, int medicoId, int pacienteId, DateTime dataHora,
        string observacoes, string diagnostico, StatusConsulta status, List<Prescricao>? prescricoes)
    {
        Id = id;
        MedicoId = medicoId;
        PacienteId = pacienteId;
        _dataHora = dataHora;
        Observacoes = observacoes;
        _diagnostico = diagnostico;
        _status = status;
        if (prescricoes != null)
            _prescricoes.AddRange(prescricoes);
    }

    // Permite que os repositórios populem as prescrições após a hidratação do objeto.
    internal void DefinirPrescricoes(IEnumerable<Prescricao> prescricoes)
    {
        _prescricoes.Clear();
        _prescricoes.AddRange(prescricoes);
    }

    public void Cancelar()
    {
        if (_status == StatusConsulta.Realizada)
            throw new InvalidOperationException("Não é possível cancelar uma consulta já realizada.");
        if (_status == StatusConsulta.Cancelada)
            throw new InvalidOperationException("A consulta já está cancelada.");
        _status = StatusConsulta.Cancelada;
    }

    public void MarcarRealizada()
    {
        if (_status == StatusConsulta.Cancelada)
            throw new InvalidOperationException("Não é possível realizar uma consulta cancelada.");
        _status = StatusConsulta.Realizada;
    }

    public void RegistrarDiagnostico(string diagnostico)
    {
        if (_status != StatusConsulta.Realizada)
            throw new InvalidOperationException("Diagnóstico só pode ser registrado em consultas realizadas.");
        if (string.IsNullOrWhiteSpace(diagnostico))
            throw new ArgumentException("Diagnóstico não pode ser vazio.");
        _diagnostico = diagnostico.Trim();
    }

    public void AdicionarPrescricao(Prescricao prescricao)
    {
        if (_status != StatusConsulta.Realizada)
            throw new InvalidOperationException("Prescrições só podem ser adicionadas em consultas realizadas.");
        _prescricoes.Add(prescricao);
    }

    public override string ToString() =>
        $"[#{Id:D4}] {DataHora:dd/MM/yyyy HH:mm} | Médico ID: {MedicoId} | Paciente ID: {PacienteId} | {Status}";
}
