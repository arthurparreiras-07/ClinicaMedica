namespace ClinicaMedica.Exceptions;

public class LimiteConsultasDiariasException : Exception
{
    public LimiteConsultasDiariasException(int medicoId, DateTime data)
        : base($"O médico ID {medicoId} já atingiu o limite de 10 consultas no dia {data:dd/MM/yyyy}.") { }
}
