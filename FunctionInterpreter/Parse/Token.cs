using System.Diagnostics;

namespace FunctionInterpreter.Parse
{
    [DebuggerDisplay("{Type}: {Text}")]
    internal class Token
    {
        public Token(string text, int start, TokenType type)
        {
            Text = text;
            Start = start;
            Type = type;
        }

        public string Text { get; }
        public TokenType Type { get; }
        public int Start { get; }
    }
}
