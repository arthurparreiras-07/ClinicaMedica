using ClinicaMedica.Interfaces;
using ClinicaMedica.Models;

namespace ClinicaMedica.Repositories;

public class PacienteRepositorio : RepositorioJson<Paciente>, IPacienteRepositorio
{
    public PacienteRepositorio(string caminhoArquivo) : base(caminhoArquivo) { }

    public override void Adicionar(Paciente paciente)
    {
        if (_dados.Any(p => p.Cpf == paciente.Cpf))
            throw new InvalidOperationException($"Já existe um paciente com o CPF '{paciente.Cpf}'.");

        paciente.Id = _dados.Count > 0 ? _dados.Max(p => p.Id) + 1 : 1;
        _dados.Add(paciente);
        Salvar();
    }

    public override Paciente? BuscarPorId(int id) =>
        _dados.FirstOrDefault(p => p.Id == id);

    public Paciente? BuscarPorCpf(string cpf)
    {
        var cpfLimpo = cpf.Replace(".", "").Replace("-", "").Trim();
        return _dados.FirstOrDefault(p => p.Cpf == cpfLimpo);
    }

    public override void Atualizar(Paciente paciente)
    {
        var index = _dados.FindIndex(p => p.Id == paciente.Id);
        if (index < 0)
            throw new KeyNotFoundException($"Paciente ID {paciente.Id} não encontrado.");
        _dados[index] = paciente;
        Salvar();
    }

    public override void Remover(int id)
    {
        var paciente = BuscarPorId(id)
            ?? throw new KeyNotFoundException($"Paciente ID {id} não encontrado.");
        _dados.Remove(paciente);
        Salvar();
    }
}
