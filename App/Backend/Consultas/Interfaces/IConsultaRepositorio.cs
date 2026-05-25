using ClinicaMedica.Backend.Consultas.Models;
using ClinicaMedica.Backend.Core.Repositories;

namespace ClinicaMedica.Backend.Consultas.Interfaces;

public interface IConsultaRepositorio : IRepositorio<Consulta>
{
    IEnumerable<Consulta> BuscarPorMedico(int medicoId);
    IEnumerable<Consulta> BuscarPorPaciente(int pacienteId);
    IEnumerable<Consulta> BuscarPorData(DateTime data);
    int ContarConsultasAtivasMedicoNoDia(int medicoId, DateTime data);
    bool PacienteTemConsultaComMedicoNoDia(int medicoId, int pacienteId, DateTime data);
}
