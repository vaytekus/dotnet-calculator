using System.Numerics;

namespace Calculator.Application.Core
{
    public class CalculatorProcessor<T>(string inputPath, string outputPath) where T : struct, INumber<T>
    {
        public void Process()
        {
            var parser = new ExpressionParser<T>(inputPath);
            using var writer = new StreamWriter(outputPath, false, System.Text.Encoding.UTF8);

            foreach (var (displayLine, sum, isValid) in parser.ParseRows())
            {
                var line = isValid ? $"{displayLine} = {sum}" : displayLine;
                Console.WriteLine(line);
                writer.WriteLine(line);
            }
        }
    }
}