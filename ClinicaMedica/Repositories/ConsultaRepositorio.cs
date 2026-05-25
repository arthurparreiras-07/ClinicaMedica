using ClinicaMedica.Interfaces;
using ClinicaMedica.Models;

namespace ClinicaMedica.Repositories;

public class ConsultaRepositorio : RepositorioJson<Consulta>, IConsultaRepositorio
{
    public ConsultaRepositorio(string caminhoArquivo) : base(caminhoArquivo) { }

    public override void Adicionar(Consulta consulta)
    {
        consulta.Id = _dados.Count > 0 ? _dados.Max(c => c.Id) + 1 : 1;
        _dados.Add(consulta);
        Salvar();
    }

    public override Consulta? BuscarPorId(int id) =>
        _dados.FirstOrDefault(c => c.Id == id);

    public IEnumerable<Consulta> BuscarPorMedico(int medicoId) =>
        _dados.Where(c => c.MedicoId == medicoId);

    public IEnumerable<Consulta> BuscarPorPaciente(int pacienteId) =>
        _dados.Where(c => c.PacienteId == pacienteId);

    public IEnumerable<Consulta> BuscarPorData(DateTime data) =>
        _dados.Where(c => c.DataHora.Date == data.Date);

    public int ContarConsultasAtivasMedicoNoDia(int medicoId, DateTime data) =>
        _dados.Count(c =>
            c.MedicoId == medicoId &&
            c.DataHora.Date == data.Date &&
            c.Status != StatusConsulta.Cancelada);

    public bool PacienteTemConsultaComMedicoNoDia(int medicoId, int pacienteId, DateTime data) =>
        _dados.Any(c =>
            c.MedicoId == medicoId &&
            c.PacienteId == pacienteId &&
            c.DataHora.Date == data.Date &&
            c.Status != StatusConsulta.Cancelada);

    public override void Atualizar(Consulta consulta)
    {
        var index = _dados.FindIndex(c => c.Id == consulta.Id);
        if (index < 0)
            throw new KeyNotFoundException($"Consulta ID {consulta.Id} não encontrada.");
        _dados[index] = consulta;
        Salvar();
    }

    public override void Remover(int id)
    {
        var consulta = BuscarPorId(id)
            ?? throw new KeyNotFoundException($"Consulta ID {id} não encontrada.");
        _dados.Remove(consulta);
        Salvar();
    }
}
