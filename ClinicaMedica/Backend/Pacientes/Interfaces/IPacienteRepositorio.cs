using ClinicaMedica.Backend.Core.Repositories;
using ClinicaMedica.Backend.Pacientes.Models;

namespace ClinicaMedica.Backend.Pacientes.Interfaces;

public interface IPacienteRepositorio : IRepositorio<Paciente>
{
    Paciente? BuscarPorCpf(string cpf);
}
