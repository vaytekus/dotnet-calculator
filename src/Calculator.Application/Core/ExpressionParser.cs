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
        private readonly Queue<string?> _output = new();
        private readonly Stack<string> _operators = new();
        private readonly Stack<T> _calcStack = new();
        private readonly Dictionary<char, int> _precedence = new()
        {
            { '+', 1 },                                              
            { '-', 1 },
            { '*', 2 },
            { '/', 2 }
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
                else
                {
                    if (_calcStack.Count < 2)
                        throw new InvalidOperationException("Exception. Wrong input.");

                    T b = _calcStack.Pop();
                    T a = _calcStack.Pop();

                    switch (token)
                    {
                        case "+": _calcStack.Push(a + b); break;
                        case "-": _calcStack.Push(a - b); break;
                        case "*": _calcStack.Push(a * b); break;
                        case "/": 
                            _calcStack.Push(a / b); 
                            break;
                    }
                }
            }

            T result = _calcStack.Pop();
            return result;
        }

        private Queue<string?> ParseLineToPostfix(string line)
        {
            _output.Clear();
            _operators.Clear();
            var matches = _defaultRegex.Matches(line);

            foreach (Match match in matches)
            {
                var token = match.Value;
                
                if (match.Groups["num"].Success && T.TryParse(token, CultureInfo.InvariantCulture, out T number))
                    _output.Enqueue(number.ToString());
                
                if (match.Groups["bra"].Success)
                {
                    if (token == "(")
                        _operators.Push(token);
                    else if (token == ")") {
                        while (_operators.Count > 0 && _operators.Peek() != "(") _output.Enqueue(_operators.Pop());
                        if (_operators.Count > 0) _operators.Pop();
                    }
                }
                
                if (match.Groups["op"].Success && _precedence.TryGetValue(Convert.ToChar(token), out int priority))
                {
                    while (_operators.Count > 0 && 
                           _precedence.TryGetValue(Convert.ToChar(_operators.Peek()), out int topPriority) && 
                           topPriority >= priority) {
                        _output.Enqueue(_operators.Pop());
                    }
                    _operators.Push(token);
                }
            }
            
            while (_operators.Count > 0) _output.Enqueue(_operators.Pop());
            
            return _output;
        }
    }
}