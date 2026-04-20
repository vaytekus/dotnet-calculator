using Calculator.Application.Core;

namespace Calculator.Application.Tests;

public class ExpressionParserTests : IDisposable
{
    private readonly string _inputPath = Path.GetTempFileName();
    private readonly string _outputPath = Path.GetTempFileName();

    private List<(string displayLine, int sum, bool isValid)> Parse(params string[] lines)
    {
        File.WriteAllLines(_inputPath, lines);
        return new ExpressionParser<int>(_inputPath).ParseRows().ToList();
    }

    [Fact]
    public void Addition_ReturnsCorrectResult()
    {
        var result = Parse("1+2");
        Assert.Single(result);
        Assert.True(result[0].isValid);
        Assert.Equal(3, result[0].sum);
    }

    [Fact]
    public void Subtraction_ReturnsCorrectResult()
    {
        var result = Parse("10-3");
        Assert.Equal(7, result[0].sum);
        Assert.True(result[0].isValid);
    }

    [Fact]
    public void Multiplication_ReturnsCorrectResult()
    {
        var result = Parse("4*5");
        Assert.Equal(20, result[0].sum);
        Assert.True(result[0].isValid);
    }

    [Fact]
    public void Division_ReturnsCorrectResult()
    {
        var result = Parse("10/2");
        Assert.Equal(5, result[0].sum);
        Assert.True(result[0].isValid);
    }

    [Theory]
    [InlineData("1+2*(3+2)", 11)]
    [InlineData("2*(3+4)", 14)]
    [InlineData("2+15/3+4*2", 15)]
    [InlineData("(2+3)*4", 20)]
    public void Brackets_ReturnsCorrectResult(string expression, int expected)
    {
        var result = Parse(expression);
        Assert.True(result[0].isValid);
        Assert.Equal(expected, result[0].sum);
    }

    [Fact]
    public void DivideByZero_ReturnsInvalidWithMessage()
    {
        var result = Parse("2/0");
        Assert.False(result[0].isValid);
        Assert.Contains("Divide by zero", result[0].displayLine);
    }

    [Fact]
    public void InvalidInput_ReturnsInvalidWithMessage()
    {
        var result = Parse("1+x+4");
        Assert.False(result[0].isValid);
        Assert.Contains("Wrong input", result[0].displayLine);
    }

    [Fact]
    public void MultipleLines_AllProcessed()
    {
        var result = Parse("1+2*(3+2)", "1+x+4", "2+15/3+4*2");
        Assert.Equal(3, result.Count);
        Assert.True(result[0].isValid);
        Assert.Equal(11, result[0].sum);
        Assert.False(result[1].isValid);
        Assert.True(result[2].isValid);
        Assert.Equal(15, result[2].sum);
    }

    public void Dispose()
    {
        File.Delete(_inputPath);
        File.Delete(_outputPath);
    }
}