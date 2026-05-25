namespace ClinicaMedica.Backend.Consultas.Exceptions;

public class ConsultaConflitanteException : Exception
{
    public ConsultaConflitanteException(string mensagem) : base(mensagem) { }
}
