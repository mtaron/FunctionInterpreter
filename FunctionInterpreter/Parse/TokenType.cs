namespace FunctionInterpreter.Parse
{
    internal enum TokenType
    {
        Unknown,
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
