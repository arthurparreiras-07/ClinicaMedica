using ClinicaMedica.Backend.Core.Models;

namespace ClinicaMedica.Backend.Medicos.Models;

public class Medico : Pessoa
{
    private string _crm = string.Empty;
    private string _especialidade = string.Empty;

    public string Crm
    {
        get => _crm;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("CRM não pode ser vazio.");
            _crm = value.Trim().ToUpper();
        }
    }

    public string Especialidade
    {
        get => _especialidade;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Especialidade não pode ser vazia.");
            _especialidade = value.Trim();
        }
    }

    public Medico() { }

    public Medico(int id, string nome, string cpf, string telefone, string crm, string especialidade)
        : base(id, nome, cpf, telefone)
    {
        Crm = crm;
        Especialidade = especialidade;
    }

    public override string ExibirInformacoes() =>
        $"Dr(a). {Nome} | CRM: {Crm} | {Especialidade} | Tel: {Telefone}";
}
