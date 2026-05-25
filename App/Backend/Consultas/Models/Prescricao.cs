namespace ClinicaMedica.Backend.Consultas.Models;

public class Prescricao
{
    public string Medicamento { get; set; } = string.Empty;
    public string Dosagem { get; set; } = string.Empty;
    public string Instrucoes { get; set; } = string.Empty;

    public Prescricao() { }

    public Prescricao(string medicamento, string dosagem, string instrucoes = "")
    {
        Medicamento = medicamento.Trim();
        Dosagem = dosagem.Trim();
        Instrucoes = instrucoes.Trim();
    }

    public override string ToString() =>
        string.IsNullOrEmpty(Instrucoes)
            ? $"{Medicamento} — {Dosagem}"
            : $"{Medicamento} — {Dosagem} ({Instrucoes})";
}
