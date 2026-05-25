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
            if (!ValidarDigitosCpf(cpfLimpo))
                throw new ArgumentException("CPF inválido. Os dígitos verificadores não conferem.");
            _cpf = cpfLimpo;
        }
    }

    private static bool ValidarDigitosCpf(string cpf)
    {
        // Rejeita sequências com todos os dígitos iguais (ex: 000.000.000-00)
        if (cpf.Distinct().Count() == 1) return false;

        var pesos1 = new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        var soma = cpf.Take(9).Select((c, i) => (c - '0') * pesos1[i]).Sum();
        var resto = soma % 11;
        if ((resto < 2 ? 0 : 11 - resto) != (cpf[9] - '0')) return false;

        var pesos2 = new[] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        soma = cpf.Take(10).Select((c, i) => (c - '0') * pesos2[i]).Sum();
        resto = soma % 11;
        return (resto < 2 ? 0 : 11 - resto) == (cpf[10] - '0');
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
