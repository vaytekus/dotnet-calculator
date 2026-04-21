# dotnet-calculator

Console app that reads math expressions from a file line by line, evaluates each one (including brackets), and writes results to an output file. Invalid expressions and division by zero are handled automatically.

## Stack

- .NET 8 / C#
- BenchmarkDotNet
- xUnit

## Run

```bash
dotnet run --project src/Calculator.Application
```

## Tests

```bash
dotnet test
```

## Benchmark

```bash
dotnet run --project src/BenchmarkDotNet.Application -c Release
```