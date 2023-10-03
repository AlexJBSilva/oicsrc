using System.Diagnostics;
using System.Text.Json;
using InterpretadorDaRinha.Environment;
using InterpretadorDaRinha.RinhaNodes;

public class Program
{
    public static async Task Main(string[] args)
    {
        string jsonFile = "/var/rinha/source.rinha.json";
        if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            jsonFile = args[0];

#if DEBUG
        Stopwatch stopWatch = new();
        stopWatch.Start();
#endif

        var serializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            MaxDepth = 2048,
        };

        using FileStream openStream = File.OpenRead(jsonFile);
        var ast = await JsonSerializer.DeserializeAsync<FileAst>(openStream, serializeOptions);
        ast.Expression.Interprete(new EnvironmentScope());

#if DEBUG
        stopWatch.Stop();
        Console.WriteLine($"Executado em: {stopWatch.Elapsed.TotalSeconds}s ({stopWatch.ElapsedMilliseconds}ms).");
#endif
    }
}