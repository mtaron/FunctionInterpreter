using System.Collections.Generic;
using System.Globalization;

namespace FunctionInterpreter.Parse
{
    internal class Scanner
    {
        private static readonly char[] ExponentTokens = new char[]{ 'E', 'e' };

        // Culture specific tokens.
        private readonly char _decimalSeperator;
        private readonly char _listSeperator;
        private readonly char _positiveSign;
        private readonly char _negativeSign;

        private readonly CompilationContext _context;
        private readonly string _text;
        private int _current = 0;
        private int _length = 0;

        private readonly List<Token> _tokens = new List<Token>();

        private Scanner(string text, CompilationContext context)
        {
            _text = text;
            _context = context;

            NumberFormatInfo numberFormat = context.CultureInfo.NumberFormat;
            _decimalSeperator = numberFormat.NumberDecimalSeparator[0];
            _positiveSign = numberFormat.PositiveSign[0];
            _negativeSign = numberFormat.NegativeSign[0];

            _listSeperator = context.CultureInfo.TextInfo.ListSeparator[0];
        }

        public static IReadOnlyList<Token> Scan(string expression, CompilationContext context)
        {
            var scanner = new Scanner(expression, context);
            return scanner.Scan();
        }

        private IReadOnlyList<Token> Scan()
        {
            ScanInternal();
            return _tokens;
        }

        private void ScanInternal()
        {
            _current = 0;
            _length = _text.Length;

            while (_current < _length)
            {
                char currentChar = _text[_current];
                if (currentChar == _decimalSeperator)
                {
                    if (char.IsDigit(NextChar()))
                    {
                        ContinueNumericLiteral(_current++);
                    }
                    else
                    {
                        AddToken(TokenType.DecimalSeperator);
                    }
                }
                else if (currentChar == _listSeperator)
                {
                    AddToken(TokenType.ListSeperator);
                }
                else if (currentChar == _positiveSign)
                {
                    AddToken(TokenType.Plus);
                }
                else if (currentChar == _negativeSign)
                {
                    AddToken(TokenType.Minus);
                }
                else
                {
                    switch (currentChar)
                    {
                        case '*':
                            AddToken(TokenType.Multiply);
                            break;
                        case '/':
                            AddToken(TokenType.Divide);
                            break;
                        case '^':
                            AddToken(TokenType.Power);
                            break;
                        case '%':
                            AddToken(TokenType.Modulus);
                            break;
                        case '(':
                            AddToken(TokenType.OpenParen);
                            break;
                        case ')':
                            AddToken(TokenType.CloseParen);
                            break;
                        default:
                            if (char.IsWhiteSpace(currentChar))
                            {
                                _current++;
                            }
                            else if (char.IsDigit(currentChar))
                            {
                                ScanNumericLiteral();
                            }
                            else if (char.IsLetter(currentChar))
                            {
                                ScanIdentifier();
                            }
                            else
                            {
                                ReportError(ErrorType.InvalidCharacter);
                                return;
                            }
                            break;
                    }
                }
            }
        }

        private void ReportError(ErrorType error)
        {
            _context.AddError(new CompileError(error, _current));
        }

        private void ScanNumericLiteral()
        {
            int start = _current;

            while (true)
            {
                _current++;
                if (_current == _length)
                {
                    AddNumericLiteral(start);
                    return;
                }

                char currentChar = _text[_current];
                if (currentChar == _decimalSeperator)
                {
                    if (char.IsDigit(NextChar()))
                    {
                        _current++;
                        ContinueNumericLiteral(start);
                        return;
                    }
                    else
                    {
                        ReportError(ErrorType.InvalidNumber);
                        _current = _length;
                        return;
                    }
                }

                if (!char.IsDigit(currentChar))
                {
                    if (IsExponent(currentChar))
                    {
                        char nextChar = NextChar();
                        if (char.IsDigit(nextChar))
                        {
                            _current++;
                            ContinueNumericLiteral(start);
                            return;
                        }
                        else if ((nextChar == _positiveSign || nextChar == _negativeSign)
                            && char.IsDigit(NextChar(lookAhead: 2)))
                        {
                            _current = _current + 2;
                            ContinueNumericLiteral(start);
                            return;
                        }
                    }

                    AddNumericLiteral(start);
                    return;
                }
            }
        }

        private void ContinueNumericLiteral(int start)
        {
            string before = _text.Substring(start, _current - start);
            start = _current;

            while (true)
            {
                _current++;
                if (_current == _length)
                {
                    AddNumericLiteral(start, before);
                    return;
                }

                char currentChar = _text[_current];
                if (!char.IsDigit(currentChar))
                {
                    if (currentChar == _decimalSeperator)
                    {
                        ReportError(ErrorType.InvalidNumber);
                        return;
                    }

                    AddNumericLiteral(start, before);
                    return;
                }
            }
        }

        private void AddNumericLiteral(int start)
        {
            var token = new Token(_text.Substring(start, _current - start), start, TokenType.NumericLiteral);
            AddToken(token);
        }

        private void AddNumericLiteral(int start, string prefix)
        {
            var token = new Token(prefix + _text.Substring(start, _current - start), start - prefix.Length, TokenType.NumericLiteral);
            AddToken(token);
        }

        private char NextChar(int lookAhead = 1)
        {
            int index = _current + lookAhead;
            if (index < _length)
            {
                return _text[index];
            }

            return '\0';
        }

        private void ScanIdentifier()
        {
            int start = _current;

            while (true)
            {
                _current++;
                if (_current == _length)
                {
                    AddIdentifier(start);
                    return;
                }

                char currentChar = _text[_current];
                if (!char.IsLetterOrDigit(currentChar))
                {
                    AddIdentifier(start);
                    return;
                }
            }
        }

        private void AddIdentifier(int start)
        {
            var token = new Token(_text.Substring(start, _current - start), start, TokenType.Identifier);
            AddToken(token);
        }

        private void AddToken(TokenType tokenType)
        {
            var token = new Token(_text[_current].ToString(), _current, tokenType);
            AddToken(token);
            _current++;
        }

        private void AddToken(in Token token)
        {
            _tokens.Add(token);
        }

        private static bool IsExponent(char character)
        {
            foreach (char exponentToken in ExponentTokens)
            {
                if (character == exponentToken)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
