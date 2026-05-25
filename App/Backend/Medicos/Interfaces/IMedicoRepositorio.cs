using ClinicaMedica.Backend.Core.Repositories;
using ClinicaMedica.Backend.Medicos.Models;

namespace ClinicaMedica.Backend.Medicos.Interfaces;

public interface IMedicoRepositorio : IRepositorio<Medico>
{
    Medico? BuscarPorCrm(string crm);
    IEnumerable<Medico> BuscarPorEspecialidade(string especialidade);
}
