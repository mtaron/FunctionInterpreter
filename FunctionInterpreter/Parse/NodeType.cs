namespace FunctionInterpreter.Parse
{
    internal enum NodeType
    {
        Unknown,
        Negation,
        Addition,
        Subtraction,
        Multiplication,
        Division,
        Modulus,
        Power,
        FunctionCall,
        Number,
        Identifier
    }
}
