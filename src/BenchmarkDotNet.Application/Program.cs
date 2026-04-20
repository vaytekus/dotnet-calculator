using BenchmarkDotNet.Running;
using Calculator.Benchmarks;

BenchmarkRunner.Run<RegexBenchmark>();
BenchmarkRunner.Run<ExpressionParserBenchmark>();