namespace ClinicaMedica.Backend.Core.Models;

public abstract class Pessoa
{
    private string _nome = string.Empty;
    private string _cpf = string.Empty;

    public int Id { get; set; }

    public string Nome
    {
        get => _nome;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Nome não pode ser vazio.");
            _nome = value.Trim();
        }
    }

    public string Cpf
    {
        get => _cpf;
        set
        {
            var cpfLimpo = value?.Replace(".", "").Replace("-", "").Trim() ?? "";
            if (cpfLimpo.Length != 11 || !cpfLimpo.All(char.IsDigit))
                throw new ArgumentException("CPF inválido. Informe 11 dígitos numéricos.");
            _cpf = cpfLimpo;
        }
    }

    public string Telefone { get; set; } = string.Empty;

    protected Pessoa() { }

    protected Pessoa(int id, string nome, string cpf, string telefone)
    {
        Id = id;
        Nome = nome;
        Cpf = cpf;
        Telefone = telefone;
    }

    public abstract string ExibirInformacoes();

    public override string ToString() => ExibirInformacoes();
}
