using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Calculator.Application.Core
{
    public class ExpressionParser<T>(string path) where T: struct, INumber<T>
    {
        private const string WrongMark = " = Exception. Wrong input.";
        private const string DivByZeroMark = " = Exception. Divide by zero.";
        private const string Pattern = @"(?<num>\d+(\.\d+)?)|(?<op>[\+\-\*/])|(?<bra>[\(\)])";
        private const string DivByZeroPattern = @"\/\s*0+(?![\d\.])";
        private static readonly Regex _defaultRegex = new Regex(Pattern, RegexOptions.Compiled);
        private static readonly Regex _divByZeroRegex = new Regex(DivByZeroPattern, RegexOptions.Compiled);
        private static readonly Regex _unaryMinusRegex = new Regex(@"(?<![0-9.)])-(\d+(?:\.\d+)?)", RegexOptions.Compiled);
        private static readonly Regex _unaryPlusRegex = new Regex(@"(?:^|(?<=\())\+", RegexOptions.Compiled);
        private readonly Queue<string?> _output = new();
        private readonly Stack<string> _operatorStack = new();
        private readonly Stack<T> _calcStack = new();
        private static readonly Dictionary<char, (int Precedence, Func<T, T, T> Op)> _operators = new()
        {
            { '+', (1, (a, b) => a + b) },
            { '-', (1, (a, b) => a - b) },
            { '*', (2, (a, b) => a * b) },
            { '/', (2, (a, b) => a / b) },
        };

        public IEnumerable<(string displayLine, T sum, bool isValid)> ParseRows()
        {
            using var reader = new StreamReader(path, System.Text.Encoding.UTF8, true, bufferSize: 65536);
            string? raw;
            while ((raw = reader.ReadLine()) != null)
            {
                (string display, T sum, bool valid) row;

                if (_divByZeroRegex.IsMatch(raw))
                {
                    row = ($"{raw}{DivByZeroMark}", T.Zero, false);
                }
                else
                {
                    var postfixLine = ParseLineToPostfix(raw);
                    try
                    {
                        T result = Calculate(postfixLine);
                        row = (raw, result, true);
                    }
                    catch (DivideByZeroException)
                    {
                        row = ($"{raw}{DivByZeroMark}", T.Zero, false);
                    }
                    catch
                    {
                        row = ($"{raw}{WrongMark}", T.Zero, false);
                    }
                }

                yield return row;
            }
        }
        
        private T Calculate(Queue<string?> postfixStr)
        {
            _calcStack.Clear();

            while (postfixStr.Count > 0)
            {
                string token = postfixStr.Dequeue();

                if (T.TryParse(token, CultureInfo.InvariantCulture, out T number))
                {
                    _calcStack.Push(number);
                }
                else if (_operators.TryGetValue(Convert.ToChar(token), out var op))
                {
                    if (_calcStack.Count < 2)
                        throw new InvalidOperationException("Exception. Wrong input.");

                    T b = _calcStack.Pop();
                    T a = _calcStack.Pop();
                    _calcStack.Push(op.Op(a, b));
                }
            }

            T result = _calcStack.Pop();
            return result;
        }

        private Queue<string?> ParseLineToPostfix(string line)
        {
            _output.Clear();
            _operatorStack.Clear();
            var normalized = line
                .Replace("--", "+").Replace("++", "+").Replace("+-", "-").Replace("-+", "-");
            normalized = _unaryMinusRegex.Replace(normalized, "(0-$1)");
            normalized = _unaryPlusRegex.Replace(normalized, "");
            var matches = _defaultRegex.Matches(normalized);

            foreach (Match match in matches)
            {
                var token = match.Value;

                if (match.Groups["num"].Success && T.TryParse(token, CultureInfo.InvariantCulture, out T number))
                    _output.Enqueue(number.ToString());

                if (match.Groups["bra"].Success)
                {
                    if (token == "(")
                        _operatorStack.Push(token);
                    else if (token == ")") {
                        while (_operatorStack.Count > 0 && _operatorStack.Peek() != "(") _output.Enqueue(_operatorStack.Pop());
                        if (_operatorStack.Count > 0) _operatorStack.Pop();
                    }
                }

                if (match.Groups["op"].Success && _operators.TryGetValue(Convert.ToChar(token), out var op))
                {
                    while (_operatorStack.Count > 0 &&
                           _operators.TryGetValue(Convert.ToChar(_operatorStack.Peek()), out var topOp) &&
                           topOp.Precedence >= op.Precedence) {
                        _output.Enqueue(_operatorStack.Pop());
                    }
                    _operatorStack.Push(token);
                }
            }

            while (_operatorStack.Count > 0) _output.Enqueue(_operatorStack.Pop());

            return _output;
        }
    }
}