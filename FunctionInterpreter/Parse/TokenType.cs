namespace FunctionInterpreter.Parse
{
    internal enum TokenType
    {
        EOF,
        NumericLiteral,
        Identifier,
        Plus,
        Minus,
        Multiply,
        Divide,
        Power,
        Modulus,
        OpenParen,
        CloseParen,
        ListSeperator,
        DecimalSeperator,
    }
}
