using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;

namespace Calculator.Benchmarks;

[MemoryDiagnoser]
public class RegexBenchmark
{
    private const string Pattern = @"(?<num>\d+(\.\d+)?)|(?<op>[\+\-\*/])|(?<bra>[\(\)])";

    private readonly Regex _interpreted = new Regex(Pattern);
    private readonly Regex _compiled = new Regex(Pattern, RegexOptions.Compiled);

    [Params("2+15/3+4*2", "1+2*(3+4*(5-2))", "100+200*300/(400-100)")]
    public string Expression { get; set; } = "";

    [Benchmark(Baseline = true)]
    public int Interpreted() => _interpreted.Matches(Expression).Count;

    [Benchmark]
    public int Compiled() => _compiled.Matches(Expression).Count;
}