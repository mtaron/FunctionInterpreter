using System.Collections.Generic;
using FunctionInterpreter.Properties;

namespace FunctionInterpreter.Parse
{
    internal class Parser
    {
        private readonly CompilationContext _context;
        private readonly IReadOnlyList<Token> _tokens;
        private readonly int _length;
        private int _current;

        private Parser(CompilationContext context, IReadOnlyList<Token> tokens)
        {
            _context = context;
            _tokens = tokens;
            _length = tokens.Count;
        }

        public static SyntaxNode Parse(string expression, CompilationContext context)
        {
            IReadOnlyList<Token> scanResult = Scanner.Scan(expression, context);
            if (context.HasErrors)
            {
                return null;
            }

            var parser = new Parser(context, scanResult);
            return parser.Parse();
        }

        private bool IsEOF
        {
            get { return _current >= _length; }
        }

        private Token Current
        {
            get
            {
                if (IsEOF)
                {
                    return null;
                }

                return _tokens[_current];
            }
        }

        private TokenType CurrentTokenType
        {
            get
            {
                if (IsEOF)
                {
                    return TokenType.Unknown;
                }

                return Current.Type;
            }
        }

        private SyntaxNode Parse()
        {
            SyntaxNode expression = ParseExpression();
            if (!_context.HasErrors && _current < _length)
            {
                ReportError(ErrorType.InvalidSyntax);
                return null;
            }

            return expression;
        }

        private SyntaxNode ParseExpression(int precedence = 0)
        {
            SyntaxNode leftOperand = null;

            if (_current == _length)
            {
                ReportError(ErrorType.ExpressionExpected);
                return null;
            }

            if (Current.Type == TokenType.Minus)
            {
                AdvanceToken();
                if (_current == _length)
                {
                    ReportError(ErrorType.InvalidTerm);
                    return null;
                }

                SyntaxNode unaryOperand = ParseExpression(GetPrecedence(NodeType.Negation));
                leftOperand = ParseNegation(unaryOperand);
            }
            else
            {
                leftOperand = ParseTerm(precedence);
            }

            while (true)
            {
                if (_current == _length)
                {
                    break;
                }

                bool isImpliedMultiplication = false;
                TokenType currentType = Current.Type;
                NodeType operationType;
                if (!IsOperatorToken(currentType))
                {
                    if (!ImpliesMultiplication(currentType))
                    {
                        break;
                    }

                    operationType = NodeType.Multiplication;
                    isImpliedMultiplication = true;
                }
                else
                {
                    operationType = GetExpressionType(currentType);
                }

                int newPrecedence = GetPrecedence(operationType);

                if (newPrecedence < precedence)
                {
                    break;
                }

                if (newPrecedence == precedence && !IsRightAssociative(operationType))
                {
                    break;
                }

                if (!isImpliedMultiplication)
                {
                    AdvanceToken();
                }

                var rightOperand = ParseExpression(newPrecedence);
                var binaryOperation = new NonterminalSyntaxNode(operationType);
                binaryOperation.Children.Add(leftOperand);
                binaryOperation.Children.Add(rightOperand);
                leftOperand = binaryOperation;
            }

            return leftOperand;
        }

        private void ReportError(string error)
        {
            int position = _length - 1;
            if (!IsEOF)
            {
                position = Current.Start;
            }

            _context.AddError(new CompileError(ErrorType.Unknown, error, position));
        }

        private void ReportError(ErrorType error)
        {
            int position = _length - 1;
            if (!IsEOF)
            {
                position = Current.Start;
            }

            _context.AddError(new CompileError(error, position));
        }

        private void AdvanceToken()
        {
            _current++;
        }

        private static bool IsOperatorToken(TokenType tokenType)
        {
            switch (tokenType)
            {
                case TokenType.Plus:
                case TokenType.Minus:
                case TokenType.Multiply:
                case TokenType.Divide:
                case TokenType.Power:
                case TokenType.Modulus:
                    return true;
                default:
                    return false;
            }
        }

        private static bool ImpliesMultiplication(TokenType currentTokenType)
        {
            return currentTokenType == TokenType.Identifier
                || currentTokenType == TokenType.OpenParen
                || currentTokenType == TokenType.NumericLiteral;
        }

        private SyntaxNode ParseTerm(int precedence)
        {
            SyntaxNode result = null;

            switch (Current.Type)
            {
                case TokenType.NumericLiteral:
                    result = ParseNumber();
                    AdvanceToken();
                    break;
                case TokenType.Identifier:
                    result = ParseIdentifier();
                    AdvanceToken();
                    if (CurrentTokenType == TokenType.OpenParen)
                    {
                        return ParseFunctionCall(result);
                    }
                    break;
                case TokenType.OpenParen:
                    AdvanceToken();
                    result = ParseExpression();
                    AdvanceToken(TokenType.CloseParen);
                    break;
                default:
                    ReportError(ErrorType.InvalidTerm);
                    break;
            }

            return result;
        }

        private SyntaxNode ParseFunctionCall(SyntaxNode functionName)
        {
            _context.SetFunctionReference(functionName.ToString());

            var result = new NonterminalSyntaxNode(NodeType.FunctionCall, functionName);
            AdvanceToken(TokenType.OpenParen);
            result.Children.Add(ParseExpression());
            while (CurrentTokenType == TokenType.ListSeperator)
            {
                AdvanceToken();
                result.Children.Add(ParseExpression());
            }

            AdvanceToken(TokenType.CloseParen);
            return result;
        }

        private void AdvanceToken(TokenType tokenType)
        {
            if (_current == _length || Current.Type != tokenType)
            {
                ReportError("Expected " + tokenType.ToString());
                return;
            }

            AdvanceToken();
        }

        private SyntaxNode ParseIdentifier()
        {
            if (IsEOF)
            {
                ReportError(ErrorType.VariableExpected);
                return null;
            }

            return new TerminalSyntaxNode(NodeType.Identifier, Current);
        }

        private SyntaxNode ParseNumber()
        {
            if (IsEOF)
            {
                ReportError(ErrorType.InvalidNumber);
                return null;
            }

            return new TerminalSyntaxNode(NodeType.Number, Current);
        }

        private SyntaxNode ParseNegation(SyntaxNode unaryOperand)
        {
            return new NonterminalSyntaxNode(NodeType.Negation, unaryOperand);
        }

        public static bool IsRightAssociative(NodeType operation)
        {
            return operation == NodeType.Power;
        }

        public static NodeType GetExpressionType(TokenType tokenType)
        {
            switch (tokenType)
            {
                case TokenType.NumericLiteral:
                    return NodeType.Number;
                case TokenType.Identifier:
                    return NodeType.Identifier;
                case TokenType.Plus:
                    return NodeType.Addition;
                case TokenType.Minus:
                    return NodeType.Subtraction;
                case TokenType.Multiply:
                    return NodeType.Multiplication;
                case TokenType.Divide:
                    return NodeType.Division;
                case TokenType.Power:
                    return NodeType.Power;
                case TokenType.Modulus:
                    return NodeType.Modulus;
                default:
                    return NodeType.Unknown;
            }
        }

        public static int GetPrecedence(NodeType nodeType)
        {
            switch (nodeType)
            {
                case NodeType.Number:
                    return 0;
                case NodeType.Addition:
                case NodeType.Subtraction:
                    return 1;
                case NodeType.Multiplication:
                case NodeType.Division:
                case NodeType.Modulus:
                    return 2;
                case NodeType.Negation:
                    return 3;
                case NodeType.Power:
                    return 4;
                case NodeType.FunctionCall:
                    return 6;
            }

            return 0;
        }
    }
}
