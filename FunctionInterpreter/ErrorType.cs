namespace FunctionInterpreter
{
    public enum ErrorType
    {
        InvalidCharacter,
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
        MissingToken,
    }
}
