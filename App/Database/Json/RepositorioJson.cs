using System.Text.Json;
using System.Text.Json.Serialization;
using ClinicaMedica.Backend.Core.Repositories;

namespace ClinicaMedica.Database.Json;

public abstract class RepositorioJson<T> : IRepositorio<T> where T : class
{
    protected List<T> _dados;
    private readonly string _caminhoArquivo;

    private static readonly JsonSerializerOptions Opcoes = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    protected RepositorioJson(string caminhoArquivo)
    {
        _caminhoArquivo = caminhoArquivo;
        _dados = Carregar();
    }

    private List<T> Carregar()
    {
        if (!File.Exists(_caminhoArquivo))
            return new List<T>();

        try
        {
            var json = File.ReadAllText(_caminhoArquivo);
            return JsonSerializer.Deserialize<List<T>>(json, Opcoes) ?? new List<T>();
        }
        catch (JsonException)
        {
            Console.WriteLine($"Aviso: arquivo de dados corrompido em '{_caminhoArquivo}'. Iniciando com lista vazia.");
            return new List<T>();
        }
    }

    public void Salvar()
    {
        var diretorio = Path.GetDirectoryName(_caminhoArquivo);
        if (!string.IsNullOrEmpty(diretorio) && !Directory.Exists(diretorio))
            Directory.CreateDirectory(diretorio);

        var json = JsonSerializer.Serialize(_dados, Opcoes);
        File.WriteAllText(_caminhoArquivo, json);
    }

    public IEnumerable<T> ListarTodos() => _dados.AsReadOnly();

    public abstract void Adicionar(T entidade);
    public abstract T? BuscarPorId(int id);
    public abstract void Atualizar(T entidade);
    public abstract void Remover(int id);
}
