using System;
using System.Diagnostics;

namespace FunctionInterpreter.Parse
{
    [DebuggerDisplay("{Type}: {Text}")]
    internal readonly struct Token
    {
        private static Token _eof = new Token(string.Empty, -1, TokenType.EOF);
        public static ref readonly Token EOF => ref _eof;

        public Token(string text, int start, TokenType type)
        {
            Text = text;
            Start = start;
            Type = type;
        }

        public string Text { get; }
        public TokenType Type { get; }
        public int Start { get; }

        public override bool Equals(object obj)
        {
            if (!(obj is Token))
            {
                return false;
            }

            var otherToken = (Token)obj;
            return otherToken.Type == Type
                && otherToken.Start == Start
                && string.Equals(otherToken.Text, Text, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode() ^ Start ^ Text.GetHashCode();
        }
    }
}
