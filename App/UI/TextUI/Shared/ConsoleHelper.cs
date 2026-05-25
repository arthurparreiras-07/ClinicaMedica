namespace ClinicaMedica.UI.TextUI.Shared;

public static class ConsoleHelper
{
    public static string Ler(string campo)
    {
        Console.Write($"  {campo}: ");
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }

    public static int LerInt(string campo)
    {
        while (true)
        {
            Console.Write($"  {campo}: ");
            if (int.TryParse(Console.ReadLine(), out var valor)) return valor;
            Erro("Valor inválido. Informe um número inteiro.");
        }
    }

    public static void Titulo(string texto) => Console.WriteLine($"\n  ── {texto} ──\n");

    public static void Sucesso(string msg) => Console.WriteLine($"\n  ✓ {msg}");
    public static void Erro(string msg)    => Console.WriteLine($"\n  ✗ Erro: {msg}");
    public static void Aviso(string msg)   => Console.WriteLine($"\n  ! {msg}");

    public static void Pausar()
    {
        Console.Write("\n  Pressione qualquer tecla para continuar...");
        Console.ReadKey();
    }
}
