using ClinicaMedica.Models;

namespace ClinicaMedica.Interfaces;

public interface IPacienteRepositorio : IRepositorio<Paciente>
{
    Paciente? BuscarPorCpf(string cpf);
}
