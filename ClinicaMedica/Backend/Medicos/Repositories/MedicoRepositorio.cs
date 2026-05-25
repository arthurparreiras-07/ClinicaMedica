using ClinicaMedica.Database.Json;
using ClinicaMedica.Backend.Medicos.Interfaces;
using ClinicaMedica.Backend.Medicos.Models;

namespace ClinicaMedica.Backend.Medicos.Repositories;

public class MedicoRepositorio : RepositorioJson<Medico>, IMedicoRepositorio
{
    public MedicoRepositorio(string caminhoArquivo) : base(caminhoArquivo) { }

    public override void Adicionar(Medico medico)
    {
        if (_dados.Any(m => m.Crm == medico.Crm))
            throw new InvalidOperationException($"Já existe um médico com o CRM '{medico.Crm}'.");

        medico.Id = _dados.Count > 0 ? _dados.Max(m => m.Id) + 1 : 1;
        _dados.Add(medico);
        Salvar();
    }

    public override Medico? BuscarPorId(int id) =>
        _dados.FirstOrDefault(m => m.Id == id);

    public Medico? BuscarPorCrm(string crm) =>
        _dados.FirstOrDefault(m => m.Crm.Equals(crm.Trim().ToUpper(), StringComparison.Ordinal));

    public IEnumerable<Medico> BuscarPorEspecialidade(string especialidade) =>
        _dados.Where(m => m.Especialidade.Contains(especialidade.Trim(), StringComparison.OrdinalIgnoreCase));

    public override void Atualizar(Medico medico)
    {
        var index = _dados.FindIndex(m => m.Id == medico.Id);
        if (index < 0)
            throw new KeyNotFoundException($"Médico ID {medico.Id} não encontrado.");
        _dados[index] = medico;
        Salvar();
    }

    public override void Remover(int id)
    {
        var medico = BuscarPorId(id)
            ?? throw new KeyNotFoundException($"Médico ID {id} não encontrado.");
        _dados.Remove(medico);
        Salvar();
    }
}
