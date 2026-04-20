using BenchmarkDotNet.Attributes;
using Calculator.Application.Core;

namespace Calculator.Benchmarks;

[MemoryDiagnoser]
public class ExpressionParserBenchmark
{
    private string _smallFile = "";
    private string _largeFile = "";

    [GlobalSetup]
    public void Setup()
    {
        _smallFile = Path.GetTempFileName();
        _largeFile = Path.GetTempFileName();

        var smallLines = new[]
        {
            "2+15/3+4*2",
            "1+2*(3+2)",
            "100+200*300/(400-100)",
            "1+x+4",
            "2/0"
        };
        File.WriteAllLines(_smallFile, smallLines);

        var random = new Random(42);
        var ops = new[] { "+", "-", "*" };
        var largeLines = Enumerable.Range(0, 1000)
            .Select(_ => $"{random.Next(1, 100)}{ops[random.Next(3)]}{random.Next(1, 100)}")
            .ToArray();
        File.WriteAllLines(_largeFile, largeLines);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        File.Delete(_smallFile);
        File.Delete(_largeFile);
    }

    [Benchmark]
    public int SmallFile_Int()
    {
        return new ExpressionParser<int>(_smallFile).ParseRows().Count();
    }

    [Benchmark]
    public int LargeFile_Int()
    {
        return new ExpressionParser<int>(_largeFile).ParseRows().Count();
    }

    [Benchmark]
    public int LargeFile_Double()
    {
        return new ExpressionParser<double>(_largeFile).ParseRows().Count();
    }
}