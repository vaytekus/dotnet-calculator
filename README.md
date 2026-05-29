# dotnet-calculator

Console app that reads math expressions from a file, evaluates them with bracket support, and writes results to an output file.

## Stack

- .NET 8 / C#
- xUnit
- BenchmarkDotNet

## Run

```bash
dotnet run --project src/Calculator.Application
```

Enter a file name when prompted — the app reads from and writes to your Documents folder (`output.txt`).

## Test

```bash
dotnet test
```

## Benchmark

```bash
dotnet run -c Release --project src/BenchmarkDotNet.Application
```