using ClinicaMedica.Models;

namespace ClinicaMedica.Interfaces;

public interface IMedicoRepositorio : IRepositorio<Medico>
{
    Medico? BuscarPorCrm(string crm);
    IEnumerable<Medico> BuscarPorEspecialidade(string especialidade);
}
