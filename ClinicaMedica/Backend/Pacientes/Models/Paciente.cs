namespace ClinicaMedica.Models;

public class Paciente : Pessoa
{
    private DateTime _dataNascimento;

    public DateTime DataNascimento
    {
        get => _dataNascimento;
        set
        {
            if (value > DateTime.Now)
                throw new ArgumentException("Data de nascimento não pode ser no futuro.");
            if (value < new DateTime(1900, 1, 1))
                throw new ArgumentException("Data de nascimento inválida.");
            _dataNascimento = value;
        }
    }

    public string Convenio { get; set; } = "Particular";

    public int Idade
    {
        get
        {
            var hoje = DateTime.Today;
            var idade = hoje.Year - DataNascimento.Year;
            if (DataNascimento.Date > hoje.AddYears(-idade)) idade--;
            return idade;
        }
    }

    public Paciente() { }

    public Paciente(int id, string nome, string cpf, string telefone, DateTime dataNascimento, string convenio = "Particular")
        : base(id, nome, cpf, telefone)
    {
        DataNascimento = dataNascimento;
        Convenio = convenio;
    }

    public override string ExibirInformacoes() =>
        $"{Nome} | {Idade} anos | Convênio: {Convenio} | Tel: {Telefone}";
}
