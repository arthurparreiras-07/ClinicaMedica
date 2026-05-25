namespace ClinicaMedica.Exceptions;

public class ConsultaConflitanteException : Exception
{
    public ConsultaConflitanteException(string mensagem) : base(mensagem) { }
}
