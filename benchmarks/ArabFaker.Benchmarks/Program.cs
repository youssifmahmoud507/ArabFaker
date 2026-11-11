using ArabFaker.NET;
using ArabFaker.NET.Export;
using ArabFaker.NET.Generation;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

// Usage: dotnet run -c Release --project benchmarks/ArabFaker.Benchmarks -- --filter "*"
// Always run benchmarks in Release; Debug numbers mean nothing.
BenchmarkSwitcher.FromAssembly(typeof(GenerationBenchmarks).Assembly).Run(args);

[MemoryDiagnoser]
public class GenerationBenchmarks
{
    private const long Seed = 2026;

    private List<BenchPerson> _people = new();

    [Params(1_000, 100_000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _people = new Faker(Seed).Define<BenchPerson>().AutoFill().Generate(Count).Value!.ToList();
    }

    [Benchmark]
    public int GenerateWithAutoFill() => new Faker(Seed).Define<BenchPerson>().AutoFill().Generate(Count).Value!.Count;

    [Benchmark]
    public int ExportJson() => RowExporter.ToJson(_people).Value!.Length;

    [Benchmark]
    public int ExportCsv() => RowExporter.ToCsv(_people).Value!.Length;

    [Benchmark]
    public int ExportSqlServer() => RowExporter.ToSql(_people, new SqlExportOptions { Dialect = SqlDialect.SqlServer, TableName = "People" }).Value!.Length;
}

public sealed class BenchPerson
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateOnly BirthDate { get; set; }
}
