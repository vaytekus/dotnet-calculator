using Calculator.Application.Core;

namespace Calculator.Application;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Enter file name:");
        string fileName = "calculations.txt";
        // string fileName = Console.ReadLine();
        
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string pathToFile = Path.Combine(documentsPath, fileName); 
        Console.WriteLine($"The file path is: {pathToFile}");
        
        if (!File.Exists(pathToFile))
        {
            Console.WriteLine("File not found.");
            return;
        }

        string outputPath = Path.Combine(documentsPath, "output.txt");

        new CalculatorProcessor<int>(pathToFile, outputPath).Process();
    }
}