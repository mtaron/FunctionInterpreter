namespace FunctionInterpreter
{
    public enum ErrorType
    {
        Unknown,
        InvalidNumber,
        ExpressionExpected,
        VariableExpected,
        InvalidTerm,
        UnknownIdentifier,
        CyclicFunctions,
        InvalidFunctionName,
        InvalidSyntax,
        UnknownFunction,
        ParenthesesRequired,
        ArgumentExpected,
        ExcessArguments,
        UnexpectedToken,
    }
}
